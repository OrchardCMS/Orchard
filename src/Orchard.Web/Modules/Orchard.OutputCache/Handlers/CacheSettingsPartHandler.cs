using System.Globalization;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.OutputCache.Handlers {
    public class CacheSettingsPartHandler : ContentHandler {
        private readonly ICacheService _cacheService;

        public CacheSettingsPartHandler(
            ICacheService cacheService) {
            _cacheService = cacheService;
            Filters.Add(new ActivatingFilter<CacheSettingsPart>("Site"));

            // initializing default cache settings values
            OnInitializing<CacheSettingsPart>((context, part) => { part.DefaultCacheDuration = 300; });

            // evict modified routable content when updated
            OnPublished<IContent>((context, part) => Invalidate(part));
        }

        private void Invalidate(IContent content) {
            // remove any page tagged with this content item id
            _cacheService.RemoveByTag(content.ContentItem.Id.ToString(CultureInfo.InvariantCulture));

            // search the cache for containers too
            var commonPart = content.As<CommonPart>();
            if (commonPart != null) {
                if (commonPart.Container != null) {
                    _cacheService.RemoveByTag(commonPart.Container.Id.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}