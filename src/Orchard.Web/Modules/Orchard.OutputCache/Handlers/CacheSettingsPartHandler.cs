using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;

namespace Orchard.OutputCache.Handlers {
    public class CacheSettingsPartHandler : ContentHandler {
        private readonly ICacheService _cacheService;

        public CacheSettingsPartHandler(
            ICacheService cacheService) {
            _cacheService = cacheService;
            Filters.Add(new ActivatingFilter<CacheSettingsPart>("Site"));

            // Default cache settings values.
            OnInitializing<CacheSettingsPart>((context, part) => {
                part.DefaultCacheDuration = 300;
                part.DefaultCacheGraceTime = 60;
            });

            OnExporting<CacheSettingsPart>(ExportRouteSettings);
            OnImporting<CacheSettingsPart>(ImportRouteSettings);
        }

        private void ExportRouteSettings(ExportContentContext context, CacheSettingsPart part) {
            var routes = _cacheService.GetRouteConfigs();
            var routesElement = new XElement("Routes",
                routes.Select(x => new XElement("Route")
                    .Attr("Key", x.RouteKey)
                    .Attr("Url", x.Url)
                    .Attr("Priority", x.Priority)
                    .Attr("Duration", x.Duration)
                    .Attr("GraceTime", x.GraceTime)
                    .Attr("MaxAge", x.MaxAge)
                    .Attr("FeatureName", x.FeatureName)));

            context.Element(part.PartDefinition.Name).Add(routesElement);
        }

        private void ImportRouteSettings(ImportContentContext context, CacheSettingsPart part) {
            var partElement = context.Data.Element(part.PartDefinition.Name);

            // Don't do anything if the tag is not specified.
            if (partElement == null)
                return;

            var routesElement = partElement.Element("Routes");

            if (routesElement == null)
                return;

            var routeConfigs = routesElement.Elements().Select(x => new CacheRouteConfig {
                RouteKey = x.Attr("Key"),
                Duration = x.Attr<int?>("Duration"),
                Priority = x.Attr<int>("Priority"),
                Url = x.Attr("Url"),
                MaxAge = x.Attr<int?>("MaxAge"),
                GraceTime = x.Attr<int?>("GraceTime"),
                FeatureName = x.Attr("FeatureName")
            });

            _cacheService.SaveRouteConfigs(routeConfigs);
        }
    }
}