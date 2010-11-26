using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Services;
using Orchard.ContentManagement;

namespace Orchard.Core.Routable.Services {
    [UsedImplicitly]
    public class RoutableHomePageProvider : IHomePageProvider {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        public const string Name = "RoutableHomePageProvider";

        public RoutableHomePageProvider(
            IOrchardServices services, 
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IWorkContextAccessor workContextAccessor) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            Services = services;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public string GetProviderName() {
            return Name;
        }

        public string GetSettingValue(int id) {
            return GetProviderName() + ";" + id;
        }

        public ActionResult GetHomePage(int itemId) {
            var contentItem = _contentManager.Get(itemId, VersionOptions.Published);
            if (contentItem == null || !contentItem.Is<RoutePart>())
                return new HttpNotFoundResult();

            // get the display metadata for the home page item
            var displayRouteValues = _contentManager.GetItemMetadata(contentItem).DisplayRouteValues;

            var model = Shape.ViewModel(RouteValues: displayRouteValues);
            return new PartialViewResult {
                ViewName = "Routable.HomePage",
                ViewData = new ViewDataDictionary<object>(model)
            };
        }
    }
}
