using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
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
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

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

            var container = _contentManager.Get(hits.Single().Id);
            IContentQuery<ContentItem> query = _contentManager
                .Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(cr => cr.Container.Id == container.Id);

            var descendingOrder = container.As<ContainerPart>().Record.OrderByDirection == (int) OrderByDirection.Descending;
            //todo: (heskew) order by custom part properties
            switch (container.As<ContainerPart>().Record.OrderByProperty) {
                case "RoutePart.Title":
                    query = descendingOrder
                        ? query.OrderByDescending<RoutePartRecord, string>(record => record.Title)
                        : query.OrderBy<RoutePartRecord, string>(record => record.Title);
                    break;
                case "RoutePart.Slug":
                    query = descendingOrder
                        ? query.OrderByDescending<RoutePartRecord, string>(record => record.Slug)
                        : query.OrderBy<RoutePartRecord, string>(record => record.Slug);
                    break;
                default: // "CommonPart.PublishedUtc"
                    query = descendingOrder
                        ? query.OrderByDescending<CommonPartRecord, DateTime?>(record => record.PublishedUtc)
                        : query.OrderBy<CommonPartRecord, DateTime?>(record => record.PublishedUtc);
                    break;
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfItems.Select(item => _contentManager.BuildDisplay(item, "Summary")));

            var viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape);

            return View(viewModel);
        }
    }
}