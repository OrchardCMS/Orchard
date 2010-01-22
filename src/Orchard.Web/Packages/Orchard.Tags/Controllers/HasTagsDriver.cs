using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;
using Orchard.Tags.Helpers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;

namespace Orchard.Tags.Controllers {

    [UsedImplicitly]
    public class HasTagsDriver : ContentPartDriver<HasTags> {
        private readonly ITagService _tagService;
        private readonly IAuthorizationService _authorizationService;

        public HasTagsDriver(ITagService tagService,
            IAuthorizationService authorizationService) {
            _tagService = tagService;
            _authorizationService = authorizationService;
        }

        public virtual IUser CurrentUser { get; set; }

        protected override DriverResult Display(HasTags part, string displayType) {
            return ContentPartTemplate(part, "Parts/Tags.ShowTags").Location("primary", "49");
        }

        protected override DriverResult Editor(HasTags part) {
            if (!_authorizationService.TryCheckAccess(Permissions.ApplyTag, CurrentUser, part))
                return null;

            var model = new EditTagsViewModel {
                Tags = string.Join(", ", part.CurrentTags.Select((t, i) => t.TagName).ToArray())
            };
            return ContentPartTemplate(model, "Parts/Tags.EditTags").Location("primary", "9");
        }

        protected override DriverResult Editor(HasTags part, IUpdateModel updater) {
            if (!_authorizationService.TryCheckAccess(Permissions.ApplyTag, CurrentUser, part))
                return null;

            var model = new EditTagsViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            var tagNames = TagHelpers.ParseCommaSeparatedTagNames(model.Tags);
            _tagService.UpdateTagsForContentItem(part.ContentItem.Id, tagNames);

            return ContentPartTemplate(model, "Parts/Tags.EditTags").Location("primary", "9");
        }
    }
}
