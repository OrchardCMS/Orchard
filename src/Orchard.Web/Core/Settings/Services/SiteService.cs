using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Services {
    [UsedImplicitly]
    public class SiteService : ISiteService {
        private readonly IRepository<SiteSettingsRecord> _siteSettingsRepository;
        private readonly IContentManager _contentManager;

        public SiteService(IRepository<SiteSettingsRecord> siteSettingsRepository, IContentManager contentManager) {
            _siteSettingsRepository = siteSettingsRepository;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ISite GetSiteSettings() {
            SiteSettingsRecord record = _siteSettingsRepository.Table.FirstOrDefault();
            if (record == null) {
                ISite site = _contentManager.Create<SiteSettings>("site", item => {
                    item.Record.SiteSalt = Guid.NewGuid().ToString("N");
                    item.Record.SiteName = "My Orchard Project Application";
                    item.Record.PageTitleSeparator = " - ";
                });
                // ensure subsequent calls will locate this object
                _contentManager.Flush();
                return site;
            }
            return _contentManager.Get<ISite>(record.Id);
        }
    }
}