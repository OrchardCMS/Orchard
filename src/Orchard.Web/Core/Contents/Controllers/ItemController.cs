using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Contents.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.Controllers {
    public class ItemController : Controller {
        private readonly IContentManager _contentManager;

        public ItemController(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        // /Contents/Item/Display/72
        public ActionResult Display(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Published);

            var model = new DisplayItemViewModel {
                Content = _contentManager.BuildDisplayModel(contentItem, "Detail")
            };
            PrepareDisplayViewModel(model.Content);
            return View("Display", model);
        }

        // /Contents/Item/Preview/72
        // /Contents/Item/Preview/72?version=5
        public ActionResult Preview(int id, int? version) {
            var versionOptions = VersionOptions.Latest;
            if (version != null) {
                versionOptions = VersionOptions.Number((int)version);
            }

            var contentItem = _contentManager.Get(id, versionOptions);

            var model = new DisplayItemViewModel {
                Content = _contentManager.BuildDisplayModel(contentItem, "Detail")
            };
            PrepareDisplayViewModel(model.Content);
            return View("Preview", model);
        }

        private static void PrepareDisplayViewModel(ContentItemViewModel itemViewModel) {
            if (string.IsNullOrEmpty(itemViewModel.TemplateName)) {
                itemViewModel.TemplateName = "Items/Contents.Item";
            }
        }
    }
}