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
        private readonly IModelManager _modelManager;

        public SiteService(IRepository<SiteSettingsRecord> siteSettingsRepository, IModelManager modelManager) {
            _siteSettingsRepository = siteSettingsRepository;
            _modelManager = modelManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of ISiteService

        public ISite GetSiteSettings() {
            string applicationName = HttpContext.Current.Request.ApplicationPath;
            SiteSettingsRecord record = _siteSettingsRepository.Get(x => x.SiteUrl == applicationName);
            if (record == null) {
                SiteModel site = _modelManager.New("site").As<SiteModel>();
                site.Record.SiteUrl = applicationName;
                _modelManager.Create(site);
                return site;
            }
            return _modelManager.Get(record.Id).As<ISite>();
        }

        #endregion
    }
}
