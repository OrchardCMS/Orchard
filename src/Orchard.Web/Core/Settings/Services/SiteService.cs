using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Caching;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Services {
    [UsedImplicitly]
    public class SiteService : ISiteService {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;

        public SiteService(
            IRepository<SiteSettingsPartRecord> siteSettingsRepository,
            IContentManager contentManager,
            ICacheManager cacheManager) {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ISite GetSiteSettings() {
            var siteId = _cacheManager.Get("SiteId", ctx => {
                var site = _contentManager.Query("Site")
                    .Slice(0, 1)
                    .FirstOrDefault();

                if (site == null) {
                    site = _contentManager.Create<SiteSettingsPart>("Site", item => {
                        item.Record.SiteSalt = Guid.NewGuid().ToString("N");
                        item.Record.SiteName = "My Orchard Project Application";
                        item.Record.PageTitleSeparator = " - ";
                        item.Record.SiteTimeZone = TimeZoneInfo.Local.Id;
                    }).ContentItem;
                }

                return site.Id;
            });

            return _contentManager.Get<ISite>(siteId, VersionOptions.Published, new QueryHints().ExpandRecords<SiteSettingsPartRecord>());
        }
    }
}