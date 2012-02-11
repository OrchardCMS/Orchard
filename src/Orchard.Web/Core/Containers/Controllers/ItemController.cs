using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Containers.Models;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Settings;
using Orchard.Localization;

namespace Orchard.Core.Containers.Controllers {

    public class ItemController : Controller {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IFeedManager _feedManager;

        public ItemController(
            IContentManager contentManager, 
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IFeedManager feedManager) {

            _contentManager = contentManager;
            _siteService = siteService;
            _feedManager = feedManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }

        public Localizer T { get; set; }
        [Themed]
        public ActionResult Display(int id, PagerParameters pagerParameters) {

            var container = _contentManager.Get(id).As<ContainerPart>();
            if (container==null)
                return HttpNotFound(T("Container not found").Text);

            // TODO: (PH) Find a way to apply PagerParameters via a driver so we can lose this controller
            container.PagerParameters = pagerParameters;
            var model = _contentManager.BuildDisplay(container, "Detail");

            return new ShapeResult(this, model);
        }

    }
}