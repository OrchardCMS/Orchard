using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentPartDriver : ContentPartDriver<CommentPart> {
        private readonly IContentManager _contentManager;
        protected override string Prefix { get { return "Comments"; } }

        public CommentPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override void Exporting(CommentPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Author", part.Record.Author);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SiteName", part.Record.SiteName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserName", part.Record.UserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Email", part.Record.Email);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Status", part.Record.Status.ToString());
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentDataUtc", part.Record.CommentDateUtc.ToString());
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentText", part.Record.CommentText);

            var commentedOn = _contentManager.Get(part.Record.CommentedOn);
            if (commentedOn != null) {
                var commentedOnIdentity = _contentManager.GetItemMetadata(commentedOn).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("CommentedOn", commentedOnIdentity.ToString());
            }

            var commentedOnContainer = _contentManager.Get(part.Record.CommentedOnContainer);
            if (commentedOnContainer != null) {
                var commentedOnContainerIdentity = _contentManager.GetItemMetadata(commentedOnContainer).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("commentedOnContainer", commentedOnContainerIdentity.ToString());
            }
        }
    }
}
