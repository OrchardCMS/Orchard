using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Settings;
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
        private readonly ICommentValidator _commentValidator;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        protected override string Prefix { get { return "Comments"; } }

        public Localizer T { get; set; }

        public CommentPartDriver(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IClock clock,
            ICommentService commentService,
            ICommentValidator commentValidator,
            IEnumerable<IHtmlFilter> htmlFilters) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _clock = clock;
            _commentValidator = commentValidator;
            _htmlFilters = htmlFilters;

            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommentPart part, string displayType, dynamic shapeHelper) {
            var formattedText = new Lazy<string>(() => {
                var commentsPart = _contentManager.Get<CommentsPart>(part.CommentedOn);
                var settings = commentsPart.TypePartDefinition.Settings.GetModel<CommentsPartSettings>();
                var formatted = _htmlFilters.Where(x => x.GetType().Name.Equals(settings.HtmlFilter, StringComparison.OrdinalIgnoreCase)).Aggregate(part.CommentText, (text, filter) => filter.ProcessContent(text));
                return formatted;
            });
            
            return Combined(
                ContentShape("Parts_Comment", () => shapeHelper.Parts_Comment(FormattedText: formattedText.Value)),
                ContentShape("Parts_Comment_SummaryAdmin", () => shapeHelper.Parts_Comment_SummaryAdmin(FormattedText: formattedText.Value))
                );
        }

        // GET
        protected override DriverResult Editor(CommentPart part, dynamic shapeHelper) {
            if (UI.Admin.AdminFilter.IsApplied(_workContextAccessor.GetContext().HttpContext.Request.RequestContext)) {
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

            var currentUser = workContext.CurrentUser;
            part.UserName = (currentUser != null ? currentUser.UserName : null);

            if (currentUser != null) part.Author = currentUser.UserName;

            if (String.IsNullOrEmpty(part.Author)) updater.AddModelError("NameMissing", T("You didn't specify your name."));

            // applying anti-spam filters
            var moderateComments = workContext.CurrentSite.As<CommentSettingsPart>().Record.ModerateComments;
            part.Status = _commentValidator.ValidateComment(part) 
                ? moderateComments ? CommentStatus.Pending : CommentStatus.Approved 
                : CommentStatus.Spam;

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

            var position = context.Attribute(part.PartDefinition.Name, "Position");
            if (position != null) {
                part.Record.Position = decimal.Parse(position, CultureInfo.InvariantCulture);
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

            var repliedOn = context.Attribute(part.PartDefinition.Name, "RepliedOn");
            if (repliedOn != null) {
                var contentItem = context.GetItemFromSession(repliedOn);
                if (contentItem != null) {
                    part.Record.RepliedOn = contentItem.Id;
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
            context.Element(part.PartDefinition.Name).SetAttributeValue("Position", part.Record.Position.ToString(CultureInfo.InvariantCulture));
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

            if (part.Record.RepliedOn.HasValue) {
                var repliedOn = _contentManager.Get(part.Record.RepliedOn.Value);
                if (repliedOn != null) {
                    var repliedOnIdentity = _contentManager.GetItemMetadata(repliedOn).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("RepliedOn", repliedOnIdentity.ToString());
                }
            }
        }
    }
}
