using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Containers.Models;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Settings;

namespace Orchard.Core.Containers.Controllers {

    public class ItemController : Controller {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public ItemController(
            IContentManager contentManager, 
            IShapeFactory shapeFactory,
            ISiteService siteService) {

            _contentManager = contentManager;
            _siteService = siteService;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        [Themed]
        public ActionResult Display(int id, PagerParameters pagerParameters)
        {
            var container = _contentManager.Get(id);
            if (container == null)
            {
                return new HttpNotFoundResult();
            }
            IContentQuery<ContentItem> query = _contentManager
                .Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(cr => cr.Container.Id == container.Id);

            var descendingOrder = container.As<ContainerPart>().Record.OrderByDirection == (int)OrderByDirection.Descending;
            query = query.OrderBy(container.As<ContainerPart>().Record.OrderByProperty, descendingOrder);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            pager.PageSize = pagerParameters.PageSize != null && container.As<ContainerPart>().Record.Paginated
                               ? pager.PageSize
                               : container.As<ContainerPart>().Record.PageSize;
            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());

            var startIndex = container.As<ContainerPart>().Record.Paginated ? pager.GetStartIndex() : 0;
            var pageOfItems = query.Slice(startIndex, pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfItems.Select(item => _contentManager.BuildDisplay(item, new DisplayOptions { DisplayType = "Summary" })));
            list.Classes.Add("content-items");
            list.Classes.Add("list-items");

            var model = _contentManager.BuildDisplay(container, new DisplayOptions { DisplayType = "Detail" });
            model.Content.Add(list, "7");
            if (container.As<ContainerPart>().Record.Paginated)
            {
                model.Content.Add(pagerShape, "7.5");
            }

            return new ShapeResult(this, model);
        }
    }
}