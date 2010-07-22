using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsPartDriver : ContentPartDriver<CommentsPart> {
        protected override DriverResult Display(CommentsPart part, string displayType) {
            if (part.CommentsShown == false) {
                return null;
            }

            // todo: (heskew) need to be more flexible with displaying parts somehow. e.g. where should the...
            // comment count go in any given skin or what if the skin builder doesn't want the count
            if (displayType.StartsWith("Detail")) {
                return ContentPartTemplate(part, "Parts/Comments.Comments").Location(part.GetLocation("Detail"));
            }
            else if (displayType == "SummaryAdmin") {
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.CountAdmin").Location(part.GetLocation("SummaryAdmin"));
            }
            else if (displayType.Contains("Summary")) {
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.Count").Location(part.GetLocation("Summary"));
            }
            else {
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.Count").Location(part.GetLocation(displayType));
            }
        }

        protected override DriverResult Editor(CommentsPart part) {
            return ContentPartTemplate(part, "Parts/Comments.Comments").Location(part.GetLocation("Editor"));
        }

        protected override DriverResult Editor(CommentsPart part, IUpdateModel updater) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part, "Parts/Comments.Comments").Location(part.GetLocation("Editor"));
        }
    }
}