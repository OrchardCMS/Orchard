using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsPartDriver : ContentPartDriver<CommentsPart> {
        protected override DriverResult Display(CommentsPart part, string displayType, dynamic shapeHelper) {
            if (part.CommentsShown == false)
                return null;

            if (displayType.StartsWith("Detail"))
                return ContentShape(shapeHelper.Parts_Comments_Comments(ContentPart: part)).Location(part.GetLocation("Detail"));

            if (displayType == "SummaryAdmin")
                return ContentShape(shapeHelper.Parts_Comments_CountAdmin(ContentPart: part, CommentCount: part.Comments.Count, PendingCount: part.PendingComments.Count))
                    .Location(part.GetLocation("SummaryAdmin"));

            var location = displayType.Contains("Summary")
                               ? part.GetLocation("Summary")
                               : part.GetLocation(displayType);

            return ContentShape(shapeHelper.Parts_Comments_Count(ContentPart: part, CommentCount: part.Comments.Count, PendingCount: part.PendingComments.Count))
                .Location(location);
        }

        protected override DriverResult Editor(CommentsPart part, dynamic shapeHelper) {
            return ContentPartTemplate(part, "Parts/Comments.Comments").Location(part.GetLocation("Editor"));
        }

        protected override DriverResult Editor(CommentsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part, "Parts/Comments.Comments").Location(part.GetLocation("Editor"));
        }
    }
}