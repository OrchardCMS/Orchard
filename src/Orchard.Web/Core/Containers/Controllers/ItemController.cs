using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Containers.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Settings;

namespace Orchard.Core.Containers.Controllers {

    public class ItemController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IContainersPathConstraint _containersPathConstraint;
        private readonly ISiteService _siteService;
        private readonly IFeedManager _feedManager;

        public ItemController(
            IContentManager contentManager, 
            IContainersPathConstraint containersPathConstraint, 
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IFeedManager feedManager) {

            _contentManager = contentManager;
            _containersPathConstraint = containersPathConstraint;
            _siteService = siteService;
            _feedManager = feedManager;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        [Themed]
        public ActionResult Display(string path, PagerParameters pagerParameters) {
            var matchedPath = _containersPathConstraint.FindPath(path);
            if (string.IsNullOrEmpty(matchedPath)) {
                throw new ApplicationException("404 - should not have passed path constraint");
            }

            var hits = _contentManager
                .Query<RoutePart, RoutePartRecord>(VersionOptions.Published)
                .Where(r => r.Path == matchedPath)
                .Slice(0, 2);

            if (hits.Count() == 0) {
                throw new ApplicationException("404 - should not have passed path constraint");
            }

            if (hits.Count() != 1) {
                throw new ApplicationException("Ambiguous content");
            }

            var container = _contentManager.Get(hits.Single().Id);
            container.As<ContainerPart>().PagerParameters = pagerParameters;
            var model = _contentManager.BuildDisplay(container, "Detail");

            return new ShapeResult(this, model);
        }
    }
}