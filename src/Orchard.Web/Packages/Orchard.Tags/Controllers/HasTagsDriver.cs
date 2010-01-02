using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Tags.Helpers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;

namespace Orchard.Tags.Controllers {

    [UsedImplicitly]
    public class HasTagsDriver : PartDriver<HasTags> {
        private readonly ITagService _tagService;

        public HasTagsDriver(ITagService tagService) {
            _tagService = tagService;
        }

        protected override DriverResult Display(HasTags part, string displayType) {
            return PartTemplate(part, "Parts/Tags.ShowTags").Location("body:above");
        }

        protected override DriverResult Editor(HasTags part) {
            var model = new EditTagsViewModel {
                Tags = string.Join(", ", part.CurrentTags.Select((t, i) => t.TagName).ToArray())
            };
            return PartTemplate(model, "Parts/Tags.EditTags").Location("primary", "9");
        }

        protected override DriverResult Editor(HasTags part, IUpdateModel updater) {

            var model = new EditTagsViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            var tagNames = TagHelpers.ParseCommaSeparatedTagNames(model.Tags);
            _tagService.UpdateTagsForContentItem(part.ContentItem.Id, tagNames);

            return PartTemplate(model, "Parts/Tags.EditTags").Location("primary", "9");
        }
    }
}
