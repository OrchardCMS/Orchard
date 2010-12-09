using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false), Themed]
    public class HomeController : Controller {
        private readonly ITagService _tagService;
        private readonly IContentManager _contentManager;
        private readonly dynamic _shapeFactory;

        public HomeController(
            ITagService tagService, 
            IContentManager contentManager, 
            IShapeFactory shapeFactory) {
            _tagService = tagService;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }
        
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var tags = _tagService.GetTags();
            var model = new TagsIndexViewModel { Tags = tags.ToList() };
            return View(model);
        }

        public ActionResult Search(string tagName, Pager pager) {
            var tag = _tagService.GetTagByName(tagName);

            if (tag == null) {
                return RedirectToAction("Index");
            }

            var taggedItems = _tagService.GetTaggedContentItems(tag.Id, pager.GetStartIndex(), pager.PageSize)
                .Select(item => _contentManager.BuildDisplay(item, "Summary"));

            var list = _shapeFactory.List();
            list.AddRange(taggedItems);

            var totalItemCount = _tagService.GetTaggedContentItemCount(tag.Id);
            var viewModel = new TagsSearchViewModel {
                TagName = tag.TagName,
                List = list,
                Pager = _shapeFactory.Pager(pager).TotalItemCount(totalItemCount)
            };

            return View(viewModel);
        }
    }
}
