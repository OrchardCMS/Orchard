using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

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
                return ContentPartTemplate(part, "Parts/Comments.HasComments").Location("primary", "after.5");
            }
            else if (displayType == "SummaryAdmin") {
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.CountAdmin").Location("meta");
            }
            else if (displayType.Contains("Summary")) {
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.Count").Location("meta", "5");
            }
            else {
                var model = new CommentCountViewModel(part);
                return ContentPartTemplate(model, "Parts/Comments.Count").Location("primary", "before.5");
            }
        }

        protected override DriverResult Editor(HasComments part) {
            return ContentPartTemplate(part, "Parts/Comments.HasComments").Location("primary", "99");
        }

        protected override DriverResult Editor(HasComments part, IUpdateModel updater) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part, "Parts/Comments.HasComments").Location("primary", "99");
        }
    }
}