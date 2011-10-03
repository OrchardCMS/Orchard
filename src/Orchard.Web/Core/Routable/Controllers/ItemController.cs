using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Routable.Controllers {
    [ValidateInput(false)]
    public class ItemController : Controller, IUpdateModel {
        private readonly ITransactionManager _transactionManager;
        private readonly IRoutablePathConstraint _routablePathConstraint;

        public ItemController(
            ITransactionManager transactionManager, 
            IRoutablePathConstraint routablePathConstraint,
            IOrchardServices services
            ) {
            _transactionManager = transactionManager;
            _routablePathConstraint = routablePathConstraint;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; private set; }

        [Themed]
        public ActionResult Display(string path) {
            var matchedPath = _routablePathConstraint.FindPath(path);
            
            if (matchedPath == null) {
                return HttpNotFound(T("Should not have passed path constraint").Text);
            }

            var hits = Services.ContentManager
                .Query<RoutePart, RoutePartRecord>(VersionOptions.Published)
                .Where(r => r.Path == matchedPath)
                .Slice(0, 2)
                .ToList();

            if (hits.Count() == 0) {
                return HttpNotFound(T("Should not have passed path constraint").Text);
            }

            if (hits.Count() != 1) {
                return HttpNotFound(T("Ambiguous content").Text);
            }

            dynamic model = Services.ContentManager.BuildDisplay(hits.Single());
            return new ShapeResult(this, model);
        }

        public ActionResult Slugify(string contentType, int? id, int? containerId) {
            const string slug = "";
            ContentItem contentItem = null;

            if (string.IsNullOrEmpty(contentType))
                return Json(slug);

            if (id != null)
                contentItem = Services.ContentManager.Get((int)id, VersionOptions.Latest);

            if (contentItem == null) {
                contentItem = Services.ContentManager.Create(contentType, VersionOptions.Draft);

                if (containerId != null) {
                    var containerItem = Services.ContentManager.Get((int)containerId);
                    contentItem.As<ICommonPart>().Container = containerItem;
                }
            }

            Services.ContentManager.UpdateEditor(contentItem, this);
            Services.ContentManager.Publish(contentItem);
            _transactionManager.Cancel();

            return Json(contentItem.As<IRoutableAspect>().GetEffectiveSlug() ?? slug);
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}