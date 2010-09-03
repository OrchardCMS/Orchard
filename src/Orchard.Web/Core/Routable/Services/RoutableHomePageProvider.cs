using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Services;
using Orchard.ContentManagement;

namespace Orchard.Core.Routable.Services {
    [UsedImplicitly]
    public class RoutableHomePageProvider : IHomePageProvider {
        private readonly IContentManager _contentManager;
        public const string Name = "RoutableHomePageProvider";

        public RoutableHomePageProvider(IOrchardServices services, IContentManager contentManager, IShapeHelperFactory shapeHelperFactory) {
            _contentManager = contentManager;
            Services = services;
            T = NullLocalizer.Instance;
            Shape = shapeHelperFactory.CreateHelper();
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
                return new NotFoundResult();

            var model = _contentManager.BuildDisplayModel(contentItem, "Detail");

            return new ViewResult {
                //ViewName = "~/Core/Routable/Views/Item/Display.ascx",
                ViewData = new ViewDataDictionary<dynamic>(Shape.HomePage(model))
            };
        }
    }
}
