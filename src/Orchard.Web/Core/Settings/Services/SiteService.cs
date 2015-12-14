using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Caching;
using Orchard.Core.Settings.Models;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Services {
    [UsedImplicitly]
    public class SiteService : ISiteService {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;

        public SiteService(
            IContentManager contentManager,
            ICacheManager cacheManager) {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ISite GetSiteSettings() {
            var siteId = _cacheManager.Get("SiteId", true, ctx => {
                var site = _contentManager.Query("Site")
                    .List()
                    .FirstOrDefault();

                if (site == null) {
                    site = _contentManager.Create<SiteSettingsPart>("Site", item => {
                        item.SiteSalt = Guid.NewGuid().ToString("N");
                        item.SiteName = "My Orchard Project Application";
                        item.PageTitleSeparator = " - ";
                        item.SiteTimeZone = TimeZoneInfo.Local.Id;
                    }).ContentItem;
                }

                return site.Id;
            });

            return _contentManager.Get<ISite>(siteId, VersionOptions.Published);
        }
    }
}