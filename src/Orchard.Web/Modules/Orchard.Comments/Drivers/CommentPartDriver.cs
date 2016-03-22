using System;
using System.Globalization;
using System.Xml;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Aspects;
using Orchard.Services;
using Orchard.Localization;
using Orchard.Comments.Services;
using Orchard.UI.Notify;

namespace Orchard.Comments.Drivers {
    public class CommentPartDriver : ContentPartDriver<CommentPart> {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IClock _clock;
        private readonly ICommentService _commentService;
        private readonly IOrchardServices _orchardServices;

        protected override string Prefix { get { return "Comments"; } }

        public Localizer T { get; set; }

        public CommentPartDriver(
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IClock clock,
            ICommentService commentService,
            IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _clock = clock;
            _commentService = commentService;
            _orchardServices = orchardServices;

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


            // applying moderate/approve actions
            var httpContext = workContext.HttpContext;
            var name = httpContext.Request.Form["submit.Save"];
            if (!string.IsNullOrEmpty(name) && String.Equals(name, "moderate", StringComparison.OrdinalIgnoreCase)) {
                if (_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't moderate comment"))) {
                    _commentService.UnapproveComment(part.Id);
                    _orchardServices.Notifier.Information(T("Comment successfully moderated."));
                    return Editor(part, shapeHelper);
                }
            }

            if (!string.IsNullOrEmpty(name) && String.Equals(name, "approve", StringComparison.OrdinalIgnoreCase)) {
                if (_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't approve comment"))) {
                    _commentService.ApproveComment(part.Id);
                    _orchardServices.Notifier.Information(T("Comment approved."));
                    return Editor(part, shapeHelper);
                }
            }

            if (!string.IsNullOrEmpty(name) && String.Equals(name, "delete", StringComparison.OrdinalIgnoreCase)) {
                if (_orchardServices.Authorizer.Authorize(Permissions.ManageComments, T("Couldn't delete comment"))) {
                    _commentService.DeleteComment(part.Id);
                    _orchardServices.Notifier.Information(T("Comment successfully deleted."));
                    return Editor(part, shapeHelper);
                }
            }

            // if editing from the admin, don't update the owner or the status
            if (!string.IsNullOrEmpty(name) && String.Equals(name, "save", StringComparison.OrdinalIgnoreCase)) {
                _orchardServices.Notifier.Information(T("Comment saved."));
                return Editor(part, shapeHelper);
            }

            part.CommentDateUtc = _clock.UtcNow;

            if (!String.IsNullOrEmpty(part.SiteName) && !part.SiteName.StartsWith("http://") && !part.SiteName.StartsWith("https://")) {
                part.SiteName = "http://" + part.SiteName;
            }

            var currentUser = workContext.CurrentUser;
            part.UserName = (currentUser != null ? currentUser.UserName : null);

            if (currentUser != null) 
                part.Author = currentUser.UserName;
            else if (string.IsNullOrWhiteSpace(part.Author)) {
                updater.AddModelError("Comments.Author", T("Name is mandatory"));
            }

            var moderateComments = workContext.CurrentSite.As<CommentSettingsPart>().ModerateComments;
            part.Status = moderateComments ? CommentStatus.Pending : CommentStatus.Approved;

            var commentedOn = _contentManager.Get<ICommonPart>(part.CommentedOn);

            // prevent users from commenting on a closed thread by hijacking the commentedOn property
            var commentsPart = commentedOn.As<CommentsPart>();
            if (commentsPart == null || !commentsPart.CommentsActive) {
                _orchardServices.TransactionManager.Cancel();
                return Editor(part, shapeHelper);
            }

            if (commentedOn != null && commentedOn.Container != null) {
                part.CommentedOnContainer = commentedOn.Container.ContentItem.Id;
            }

            commentsPart.Record.CommentPartRecords.Add(part.Record);

            return Editor(part, shapeHelper);
        }

        protected override void Importing(CommentPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Author", author =>
                part.Record.Author = author
            );

            context.ImportAttribute(part.PartDefinition.Name, "SiteName", siteName =>
                part.Record.SiteName = siteName
            );

            context.ImportAttribute(part.PartDefinition.Name, "UserName", userName =>
                part.Record.UserName = userName
            );

            context.ImportAttribute(part.PartDefinition.Name, "Email", email =>
                part.Record.Email = email
            );

            context.ImportAttribute(part.PartDefinition.Name, "Position", position =>
                part.Record.Position = decimal.Parse(position, CultureInfo.InvariantCulture)
            );

            context.ImportAttribute(part.PartDefinition.Name, "Status", status =>
                part.Record.Status = (CommentStatus)Enum.Parse(typeof(CommentStatus), status)
            );

            context.ImportAttribute(part.PartDefinition.Name, "CommentDateUtc", commentDate =>
                part.Record.CommentDateUtc = XmlConvert.ToDateTime(commentDate, XmlDateTimeSerializationMode.Utc)
            );

            context.ImportAttribute(part.PartDefinition.Name, "CommentText", text =>
                part.Record.CommentText = text
            );

            context.ImportAttribute(part.PartDefinition.Name, "CommentedOn", commentedOn => {
                var contentItem = context.GetItemFromSession(commentedOn);
                if (contentItem != null) {
                    part.Record.CommentedOn = contentItem.Id;
                }

                contentItem.As<CommentsPart>().Record.CommentPartRecords.Add(part.Record);
            });

            context.ImportAttribute(part.PartDefinition.Name, "RepliedOn", repliedOn => {
                var contentItem = context.GetItemFromSession(repliedOn);
                if (contentItem != null) {
                    part.Record.RepliedOn = contentItem.Id;
                }
            });

            context.ImportAttribute(part.PartDefinition.Name, "CommentedOnContainer", commentedOnContainer => {
                var container = context.GetItemFromSession(commentedOnContainer);
                if (container != null) {
                    part.Record.CommentedOnContainer = container.Id;
                }
            });
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
