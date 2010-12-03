using System;
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
        public const string Name = "RoutableHomePageProvider";

        public RoutableHomePageProvider(
            IContentManager contentManager,
            IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public string GetProviderName() {
            return Name;
        }

        public string GetSettingValue(int id) {
            return GetProviderName() + ";" + id;
        }

        public int GetHomePageId(string value) {
            int id;

            if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value.Substring(Name.Length + 1), out id))
                throw new ApplicationException(T("Invalid home page setting value for {0}: {1}", Name, value).Text);

            return id;
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
