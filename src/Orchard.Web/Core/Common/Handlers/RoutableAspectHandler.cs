using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Services;
using Orchard.Data;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class RoutableAspectHandler : ContentHandler {
        private readonly IEnumerable<IContentItemDriver> _contentItemDrivers;

        public RoutableAspectHandler(IRepository<RoutableRecord> repository, IEnumerable<IContentItemDriver> contentItemDrivers, IRoutableService routableService, UrlHelper urlHelper) {
            _contentItemDrivers = contentItemDrivers;

            Filters.Add(StorageFilter.For(repository));

            OnGetEditorViewModel<RoutableAspect>((context, routable) => {
                var currentDriver = GetDriver(context.ContentItem);
                var tempContentItem = context.ContentItem.ContentManager.New(context.ContentItem.ContentType);
                tempContentItem.As<RoutableAspect>().Slug = "ABCDEFG";

                var routeValues = GetRouteValues(currentDriver, tempContentItem);
                var url = urlHelper.RouteUrl(routeValues).Replace("/ABCDEFG", "");

                if (url.StartsWith("/"))
                    url = url.Substring(1);

                routable.ContentItemBasePath = url;
            });

            OnCreated<RoutableAspect>((context, ra) => routableService.ProcessSlug(ra));

            OnIndexing<RoutableAspect>((context, part) => context.IndexDocument
                                                    .Add("slug", part.Slug)
                                                    .Add("title", part.Title)
                                                    );
        }

        private static RouteValueDictionary GetRouteValues(IContentItemDriver driver, ContentItem contentItem) {
            //TODO: (erikpo) Need to rearrange ContentItemDriver so reflection isn't needed here
            var driverType = driver.GetType();
            var method = driverType.GetMethod("GetDisplayRouteValues");

            if (method != null) {
                return (RouteValueDictionary)method.Invoke(driver, new object[] {contentItem.Get(driverType.BaseType.GetGenericArguments()[0])});
            }

            return null;
        }

        private IContentItemDriver GetDriver(ContentItem contentItem) {
            return
                _contentItemDrivers
                .Where(cid => cid.GetContentTypes().Any(ct => string.Compare(ct.Name, contentItem.ContentType, true) == 0))
                //TODO: (erikpo) SingleOrDefault should be called here, but for some reason, the amount of drivers registered is doubled sometimes.
                .FirstOrDefault();
        }
    }
}