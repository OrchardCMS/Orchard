using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.OutputCache.Services;

namespace Orchard.OutputCache.Handlers {
    public class CacheItemInvalidationHandler : ContentHandler {
        private readonly ICacheService _cacheService;

        public CacheItemInvalidationHandler(ICacheService cacheService) {
            _cacheService = cacheService;

            // Evict cached content when updated, removed or destroyed.
            OnPublished<IContent>((context, part) => Invalidate(part));
            OnRemoved<IContent>((context, part) => Invalidate(part));
            OnDestroyed<IContent>((context, part) => Invalidate(part));
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