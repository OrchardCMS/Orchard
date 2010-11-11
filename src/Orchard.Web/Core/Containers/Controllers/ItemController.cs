using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Orchard.Core.Containers.Controllers {
    public class ItemController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IContainersPathConstraint _containersPathConstraint;

        public ItemController(IContentManager contentManager, IContainersPathConstraint containersPathConstraint, IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            _containersPathConstraint = containersPathConstraint;
            New = shapeFactory;
        }

        dynamic New { get; set; }

        [Themed]
        public ActionResult Display(string path, Pager pager) {
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

            var containerId = hits.Single().Id;
            var items = _contentManager
                .Query<ContentPart<CommonPartRecord>, CommonPartRecord>(VersionOptions.Published)
                .Where(x => x.Container.Id == containerId)
                .List();

            var itemDisplays = items.Select(item => _contentManager.BuildDisplay(item, "Summary"));
            var list = New.List();
            list.AddRange(itemDisplays);

            return View(list);
        }
    }
}