using System.Linq;
using Orchard.Caching;
using Orchard.Core.Settings.Models;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Services {
    public class SiteService : ISiteService {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;

        public SiteService(
            IContentManager contentManager,
            ICacheManager cacheManager) {

            _contentManager = contentManager;
            _cacheManager = cacheManager;

        }

        public ISite GetSiteSettings() {
            SiteSettingsCache siteSettingsCache = _cacheManager.Get("SiteSettings",
                ctx => new SiteSettingsCache(GetSiteSettingsPart()));
            siteSettingsCache.ResetCache(this);
            return siteSettingsCache;
        }

        public ISite GetSiteSettingsPart() {
            var siteId = _cacheManager.Get("SiteId", ctx => {
                var site = _contentManager.Query("Site")
                    .Slice(0, 1)
                    .FirstOrDefault();

                if (site == null) {
                    site = _contentManager.Create<SiteSettingsPart>("Site").ContentItem;
                }

                return site.Id;
            });

            return _contentManager.Get<ISite>(siteId);
        }
    }
}