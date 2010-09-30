using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;
using Orchard.Themes;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false), Themed]
    public class HomeController : Controller {
        private readonly ITagService _tagService;
        private readonly IContentManager _contentManager;
        private readonly IShapeHelperFactory _shapeHelperFactory;

        public HomeController(ITagService tagService, IContentManager contentManager, IShapeHelperFactory shapeHelperFactory) {
            _tagService = tagService;
            _contentManager = contentManager;
            _shapeHelperFactory = shapeHelperFactory;
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

            var shape = _shapeHelperFactory.CreateHelper();
            var list = shape.List();
            foreach (var taggedContentItem in _tagService.GetTaggedContentItems(tag.Id)) {
                list.Add(_contentManager.BuildDisplayModel(taggedContentItem, "Summary"));
            }

            var viewModel = new TagsSearchViewModel {
                TagName = tag.TagName,
                List = list
            };

            return View(viewModel);
        }
    }
}
