using System.Linq;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using System.Web;

namespace Orchard.Core.Settings.Services {
    public class SiteService : ISiteService {
        private readonly IRepository<SiteSettingsRecord> _siteSettingsRepository;
        private readonly IContentManager _contentManager;

        public SiteService(IRepository<SiteSettingsRecord> siteSettingsRepository, IContentManager contentManager) {
            _siteSettingsRepository = siteSettingsRepository;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of ISiteService

        public ISite GetSiteSettings() {
            string applicationName = HttpContext.Current.Request.ApplicationPath;
            SiteSettingsRecord record = _siteSettingsRepository.Fetch(x => x.SiteUrl == applicationName).FirstOrDefault();
            if (record == null) {
                ISite site = _contentManager.Create<SiteSettings>("site", item => {
                    item.Record.SiteUrl = applicationName;
                });
                return site;
            }
            return _contentManager.Get<ISite>(record.Id);
        }

        #endregion
    }
}
