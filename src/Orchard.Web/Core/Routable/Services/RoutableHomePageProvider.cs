using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.ViewModels;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Services;
using Orchard.ContentManagement;

namespace Orchard.Core.Routable.Services {
    [UsedImplicitly]
    public class RoutableHomePageProvider : IHomePageProvider {
        private readonly IContentManager _contentManager;

        public RoutableHomePageProvider(IOrchardServices services, IContentManager contentManager) {
            _contentManager = contentManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public string GetProviderName() {
            return "RoutableHomePageProvider";
        }

        public ActionResult GetHomePage(int itemId) {
            var contentItem = _contentManager.Get(itemId, VersionOptions.Published);
            if (contentItem == null || !contentItem.Is<RoutePart>())
                return new NotFoundResult();

            var model = new RoutableDisplayViewModel {
                Routable = _contentManager.BuildDisplayModel<IRoutableAspect>(contentItem.As<RoutePart>(), "Detail")
            };

            return new ViewResult {
                ViewName = "~/Core/Routable/Views/Item/Display.ascx",
                ViewData = new ViewDataDictionary<RoutableDisplayViewModel>(model)
            };
        }
    }
}
