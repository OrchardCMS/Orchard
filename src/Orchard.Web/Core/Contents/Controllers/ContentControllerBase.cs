using System;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Mvc.Extensions;

namespace Orchard.Core.Contents.Controllers {
    public abstract class ContentControllerBase : Controller {
        private readonly IContentManager _contentManager;


        public ContentControllerBase(IContentManager contentManager) {
            _contentManager = contentManager;
        }


        public RedirectToRouteResult GetCustomContentItemRouteRedirection(IContent content, ContentItemRoute contentItemRoute) {
            if (content == null) return null;

            var itemMetadata = _contentManager.GetItemMetadata(content);
            var currentRoute = RouteData.Values;

            bool isCustomRoute(RouteValueDictionary routeValues) =>
                !currentRoute.ToRouteString().Equals(routeValues.ToRouteString(), StringComparison.InvariantCultureIgnoreCase);

            switch (contentItemRoute) {
                case ContentItemRoute.Admin:
                    if (isCustomRoute(itemMetadata.AdminRouteValues))
                        return RedirectToRoute(itemMetadata.AdminRouteValues);

                    break;
                case ContentItemRoute.Editor:
                    if (isCustomRoute(itemMetadata.EditorRouteValues))
                        return RedirectToRoute(itemMetadata.EditorRouteValues);

                    break;
                case ContentItemRoute.Create:
                    if (isCustomRoute(itemMetadata.CreateRouteValues))
                        return RedirectToRoute(itemMetadata.CreateRouteValues);

                    break;
                case ContentItemRoute.Display:
                    if (isCustomRoute(itemMetadata.DisplayRouteValues))
                        return RedirectToRoute(itemMetadata.DisplayRouteValues);

                    break;
            }

            return null;
        }
    }
}