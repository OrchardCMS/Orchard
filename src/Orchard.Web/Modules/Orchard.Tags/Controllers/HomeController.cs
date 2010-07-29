using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false)]
    public class HomeController : Controller {
        private readonly ITagService _tagService;
        private readonly IContentManager _contentManager;

        public HomeController(ITagService tagService, IContentManager contentManager) {
            _tagService = tagService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var tags = _tagService.GetTags();
            var model = new TagsIndexViewModel { Tags = tags.ToList() };
            return View(model);
        }

        public ActionResult Search(string tagName) {
            var tag = _tagService.GetTagByName(tagName);

            if (tag == null) {
                return RedirectToAction("Index");
            }

            var items =
                _tagService.GetTaggedContentItems(tag.Id).Select(
                    ic => _contentManager.BuildDisplayModel(ic, "SummaryForSearch"));

            var viewModel = new TagsSearchViewModel {
                TagName = tag.TagName,
                Items = items.ToList()
            };

            return View(viewModel);
        }
    }
}
