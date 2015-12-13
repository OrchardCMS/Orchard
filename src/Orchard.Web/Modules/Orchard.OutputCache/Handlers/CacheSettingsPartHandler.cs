using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
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

            // Evict modified routable content when updated.
            OnPublished<IContent>((context, part) => Invalidate(part));
        }

        private void Invalidate(IContent content) {
            // Remove any item tagged with this content item ID.
            _cacheService.RemoveByTag(content.ContentItem.Id.ToString(CultureInfo.InvariantCulture));

            // Search the cache for containers too.
            var commonPart = content.As<CommonPart>();
            if (commonPart != null) {
                if (commonPart.Container != null) {
                    _cacheService.RemoveByTag(commonPart.Container.Id.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}