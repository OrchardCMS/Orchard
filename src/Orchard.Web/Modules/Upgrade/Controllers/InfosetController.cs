using System.Linq;
using System.Security.Authentication;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Upgrade.Services;

namespace Upgrade.Controllers {
    [Admin]
    public class InfosetController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IUpgradeService _upgradeService;

        private const int BATCH = 50;

        public InfosetController(
            IOrchardServices orchardServices,
            IUpgradeService upgradeService) {
            _orchardServices = orchardServices;
            _upgradeService = upgradeService;

            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        
        public ActionResult Index() {
            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var site = _orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>();

            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettingsPartRecord"),
                (reader, connection) => {
                    site.HomePage = (string)reader["HomePage"];
                    site.PageSize = (int)reader["PageSize"];
                    site.PageTitleSeparator = (string)reader["PageTitleSeparator"];
                    site.ResourceDebugMode = (ResourceDebugMode)reader["ResourceDebugMode"];
                    site.SiteCulture = (string)reader["SiteCulture"];
                    site.SiteName = (string)reader["SiteName"];
                    site.SiteSalt = (string)reader["SiteSalt"];
                    site.SiteTimeZone = (string)reader["SiteTimeZone"];
                    site.SuperUser = (string)reader["SuperUser"];
                });

            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettings2PartRecord"),
                (reader, connection) => {
                    site.BaseUrl = (string)reader["BaseUrl"];
                });

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettingsPartRecord"), null);
            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettings2PartRecord"), null);
            
            _orchardServices.Notifier.Information(T("Site Settings migrated successfully"));

            return View();
        }

        [HttpPost]
        public JsonResult MigrateBody(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var contentItems = _orchardServices.ContentManager
                .Query<BodyPart, BodyPartRecord>()
                .Where(x => x.Id > id)
                .OrderBy(x => x.Id)
                .Slice(0, BATCH).ToList();

            foreach (var contentItem in contentItems) {
                contentItem.Text = contentItem.Text;
            }

            return new JsonResult { Data = contentItems.Last().Id };
        }
    }
}
