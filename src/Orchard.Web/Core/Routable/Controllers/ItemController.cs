using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Themes;

namespace Orchard.Core.Routable.Controllers {
    [ValidateInput(false)]
    public class ItemController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IRoutablePathConstraint _routablePathConstraint;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHomePageProvider _routableHomePageProvider;

        public ItemController(
            IContentManager contentManager, 
            ITransactionManager transactionManager, 
            IRoutablePathConstraint routablePathConstraint,
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor,
            IEnumerable<IHomePageProvider> homePageProviders) {
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _routablePathConstraint = routablePathConstraint;
            _workContextAccessor = workContextAccessor;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        [Themed]
        public ActionResult Display(string path) {
            var matchedPath = _routablePathConstraint.FindPath(path);
            if (matchedPath == null) {
                throw new ApplicationException(T("404 - should not have passed path constraint").Text);
            }

            var hits = _contentManager
                .Query<RoutePart, RoutePartRecord>(VersionOptions.Published)
                .Where(r => r.Path == matchedPath)
                .Slice(0, 2);
            if (hits.Count() == 0) {
                throw new ApplicationException(T("404 - should not have passed path constraint").Text);
            }
            if (hits.Count() != 1) {
                throw new ApplicationException(T("Ambiguous content").Text);
            }

            var item = hits.Single();
            // primary action run for a home paged item shall not pass
            if (!RouteData.DataTokens.ContainsKey("ParentActionViewContext")
                && item.Id == _routableHomePageProvider.GetHomePageId(_workContextAccessor.GetContext().CurrentSite.HomePage)) {
                return HttpNotFound();
            }

            dynamic model = _contentManager.BuildDisplay(item);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        public ActionResult Slugify(string contentType, int? id, int? containerId) {
            const string slug = "";
            ContentItem contentItem = null;

            if (string.IsNullOrEmpty(contentType))
                return Json(slug);

            if (id != null)
                contentItem = _contentManager.Get((int)id, VersionOptions.Latest);

            if (contentItem == null) {
                contentItem = _contentManager.Create(contentType, VersionOptions.Draft);

                if (containerId != null) {
                    var containerItem = _contentManager.Get((int)containerId);
                    contentItem.As<ICommonPart>().Container = containerItem;
                }
            }

            _contentManager.UpdateEditor(contentItem, this);
            _contentManager.Publish(contentItem);
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