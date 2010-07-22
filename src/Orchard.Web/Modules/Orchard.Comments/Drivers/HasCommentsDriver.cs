using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class HasCommentsDriver : ContentPartDriver<HasComments> {
        protected override DriverResult Display(HasComments part, string displayType) {
            if (part.CommentsShown == false) {
                return null;
            }

            // todo: (heskew) need to be more flexible with displaying parts somehow. e.g. where should the...
            // comment count go in any given skin or what if the skin builder doesn't want the count
            if (displayType.StartsWith("Detail")) {
                //return Combined(
                //    ContentPartTemplate(part, "Parts/Comments.Count").Location("body", "above.5"),
                //    ContentPartTemplate(part, "Parts/Comments.HasComments").Location("body", "below.5"));
                var location = part.GetLocation("Detail", "primary", "after.5");
                return ContentPartTemplate(part, "Parts/Comments.HasComments").Location(location);
            }
            else if (displayType == "SummaryAdmin") {
                var location = part.GetLocation("SummaryAdmin", "meta", null);
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.CountAdmin").Location(location);
            }
            else if (displayType.Contains("Summary")) {
                var location = part.GetLocation("Summary", "meta", "5");
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.Count").Location(location);
            }
            else {
                var location = part.GetLocation(displayType, "primary", "before.5");
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.Count").Location(location);
            }
        }

        protected override DriverResult Editor(HasComments part) {
            var location = part.GetLocation("Editor", "primary", "10");
            return ContentPartTemplate(part, "Parts/Comments.HasComments").Location(location);
        }

        protected override DriverResult Editor(HasComments part, IUpdateModel updater) {
            var location = part.GetLocation("Editor", "primary", "10");
            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part, "Parts/Comments.HasComments").Location(location);
        }
    }
}