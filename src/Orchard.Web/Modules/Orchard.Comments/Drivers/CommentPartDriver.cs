using System;
using System.Xml;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Aspects;
using Orchard.Services;
using Orchard.Localization;
using Orchard.Comments.Services;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentPartDriver : ContentPartDriver<CommentPart> {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IClock _clock;
        private readonly ICommentService _commentService;

        protected override string Prefix { get { return "Comments"; } }

        public Localizer T { get; set; }

        public CommentPartDriver(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IClock clock,
            ICommentService commentService) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _clock = clock;
            _commentService = commentService;

            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommentPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Comment", () => shapeHelper.Parts_Comment()),
                ContentShape("Parts_Comment_SummaryAdmin", () => shapeHelper.Parts_Comment_SummaryAdmin())
                );
        }

        // GET
        protected override DriverResult Editor(CommentPart part, dynamic shapeHelper) {
            if (Orchard.UI.Admin.AdminFilter.IsApplied(_workContextAccessor.GetContext().HttpContext.Request.RequestContext)) {
                return ContentShape("Parts_Comment_AdminEdit", 
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts.Comment.AdminEdit", Model: part, Prefix: Prefix));
            }
            else {
                return ContentShape("Parts_Comment_Edit", 
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts.Comment", Model: part, Prefix: Prefix));
	        }
        }

        // POST
        protected override DriverResult Editor(CommentPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            var workContext = _workContextAccessor.GetContext();

            part.CommentDateUtc = _clock.UtcNow;

            if (!String.IsNullOrEmpty(part.SiteName) && !part.SiteName.StartsWith("http://") && !part.SiteName.StartsWith("https://")) {
                part.SiteName = "http://" + part.SiteName;
            }

            // TODO: it's very bad how the corresponding user is stored. Needs revision.
            var currentUser = workContext.CurrentUser;
            part.UserName = (currentUser != null ? currentUser.UserName : null);

            if (currentUser != null) part.Author = currentUser.UserName;

            if (String.IsNullOrEmpty(part.Author)) updater.AddModelError("NameMissing", T("You didn't specify your name."));

            // TODO: needs spam handling
            part.Status = workContext.CurrentSite.As<CommentSettingsPart>().ModerateComments ? CommentStatus.Pending : CommentStatus.Approved;

            var commentedOn = _contentManager.Get<ICommonPart>(part.CommentedOn);
            if (commentedOn != null && commentedOn.Container != null) {
                part.CommentedOnContainer = commentedOn.Container.ContentItem.Id;
            }
            commentedOn.As<CommentsPart>().Record.CommentPartRecords.Add(part.Record);

            return Editor(part, shapeHelper);
        }

        protected override void Importing(CommentPart part, ContentManagement.Handlers.ImportContentContext context) {
            var author = context.Attribute(part.PartDefinition.Name, "Author");
            if (author != null) {
                part.Record.Author = author;
            }

            var siteName = context.Attribute(part.PartDefinition.Name, "SiteName");
            if (siteName != null) {
                part.Record.SiteName = siteName;
            }

            var userName = context.Attribute(part.PartDefinition.Name, "UserName");
            if (userName != null) {
                part.Record.UserName = userName;
            }

            var email = context.Attribute(part.PartDefinition.Name, "Email");
            if (email != null) {
                part.Record.Email = email;
            }

            var status = context.Attribute(part.PartDefinition.Name, "Status");
            if (status != null) {
                part.Record.Status = (CommentStatus)Enum.Parse(typeof(CommentStatus), status);
            }

            var commentDate = context.Attribute(part.PartDefinition.Name, "CommentDateUtc");
            if (commentDate != null) {
                part.Record.CommentDateUtc = XmlConvert.ToDateTime(commentDate, XmlDateTimeSerializationMode.Utc);
            }

            var text = context.Attribute(part.PartDefinition.Name, "CommentText");
            if (text != null) {
                part.Record.CommentText = text;
            }

            var commentedOn = context.Attribute(part.PartDefinition.Name, "CommentedOn");
            if (commentedOn != null) {
                var contentItem = context.GetItemFromSession(commentedOn);
                if (contentItem != null) {
                    part.Record.CommentedOn = contentItem.Id;
                }

                contentItem.As<CommentsPart>().Record.CommentPartRecords.Add(part.Record);
            }

            var commentedOnContainer = context.Attribute(part.PartDefinition.Name, "CommentedOnContainer");
            if (commentedOnContainer != null) {
                var container = context.GetItemFromSession(commentedOnContainer);
                if (container != null) {
                    part.Record.CommentedOnContainer = container.Id;
                }
            }
        }

        protected override void Exporting(CommentPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Author", part.Record.Author);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SiteName", part.Record.SiteName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserName", part.Record.UserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Email", part.Record.Email);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Status", part.Record.Status.ToString());

            if (part.Record.CommentDateUtc != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("CommentDateUtc", XmlConvert.ToString(part.Record.CommentDateUtc.Value, XmlDateTimeSerializationMode.Utc));
            }
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentText", part.Record.CommentText);

            var commentedOn = _contentManager.Get(part.Record.CommentedOn);
            if (commentedOn != null) {
                var commentedOnIdentity = _contentManager.GetItemMetadata(commentedOn).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("CommentedOn", commentedOnIdentity.ToString());
            }

            var commentedOnContainer = _contentManager.Get(part.Record.CommentedOnContainer);
            if (commentedOnContainer != null) {
                var commentedOnContainerIdentity = _contentManager.GetItemMetadata(commentedOnContainer).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("CommentedOnContainer", commentedOnContainerIdentity.ToString());
            }
        }
    }
}
