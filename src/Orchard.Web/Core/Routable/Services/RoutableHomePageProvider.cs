using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Services;
using Orchard.ContentManagement;

namespace Orchard.Core.Routable.Services {
    [UsedImplicitly]
    public class RoutableHomePageProvider : IHomePageProvider {
        private readonly IContentManager _contentManager;
        public const string Name = "RoutableHomePageProvider";

        public RoutableHomePageProvider(
            IContentManager contentManager,
            IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        public string GetProviderName() {
            return Name;
        }

        public string GetSettingValue(int id) {
            return GetProviderName() + ";" + id;
        }

        public ActionResult GetHomePage(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Published);
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
