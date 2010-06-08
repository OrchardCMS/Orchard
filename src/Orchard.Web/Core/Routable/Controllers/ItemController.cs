using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Routable.Controllers {
    [ValidateInput(false)]
    public class ItemController : Controller {
        private readonly IContentManager _contentManager;

        public ItemController(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ActionResult Display(string path) {
            var hits = _contentManager
                .Query<IsRoutable, RoutableRecord>(VersionOptions.Published)
                .Where(r => r.Path == path)
                .Slice(0, 2);
            if (hits.Count() == 0) {
                throw new ApplicationException("404 - should not have passed path constraint");
            }
            if (hits.Count() != 1) {
                throw new ApplicationException("Ambiguous content");
            }
            var model = new RoutableDisplayViewModel {
                Routable = _contentManager.BuildDisplayModel<IRoutableAspect>(hits.Single(), "Detail")
            };
            PrepareDisplayViewModel(model.Routable);
            return View("Display", model);
        }
        
        private void PrepareDisplayViewModel(ContentItemViewModel<IRoutableAspect> itemViewModel) {
            if (string.IsNullOrEmpty(itemViewModel.TemplateName)) {
                itemViewModel.TemplateName = "Items/Contents.Item";
            }
        }
    }
}