using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
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

            // SiteSettingsPartRecord
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

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettingsPartRecord"), null);

            // SiteSettings2PartRecord
            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettings2PartRecord"),
                (reader, connection) => {
                    site.BaseUrl = (string)reader["BaseUrl"];
                });

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_Core_SiteSettings2PartRecord"), null);

            // ThemeSiteSettingsPartRecord
            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Themes_ThemeSiteSettingsPartRecord"),
                (reader, connection) => site.As<InfosetPart>().Store("ThemeSiteSettingsPart", "CurrentThemeName", (string)reader["CurrentThemeName"]));

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_Themes_ThemeSiteSettingsPartRecord"), null);


            // AkismetSettingsPartRecord
            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_AkismetSettingsPartRecord"),
                (reader, connection) => {
                    site.As<InfosetPart>().Store("AkismetSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                    site.As<InfosetPart>().Store("AkismetSettingsPart", "ApiKey", reader["ApiKey"].ToString());
                });

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_AkismetSettingsPartRecord"), null);

            // ReCaptchaSettingsPartRecord
            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_ReCaptchaSettingsPartRecord"),
                (reader, connection) => {
                    site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "PublicKey", reader["PublicKey"].ToString());
                    site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "PrivateKey", reader["PrivateKey"].ToString());
                    site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                });

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_ReCaptchaSettingsPartRecord"), null);

            // TypePadSettingsPartRecord
            _upgradeService.ExecuteReader("SELECT * FROM " + _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_TypePadSettingsPartRecord"),
                (reader, connection) => {
                    site.As<InfosetPart>().Store("TypePadSettingsPart", "ApiKey", reader["ApiKey"].ToString());
                    site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                });

            _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_TypePadSettingsPartRecord"), null);


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


        [HttpPost]
        public JsonResult MigrateContentPermissionsPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;
            
            _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + _upgradeService.GetPrefixedTableName("Orchard_ContentPermissions_ContentPermissionsPartRecord") + " WHERE Id > " + id,
                (reader, connection) => {
                    lastContentItemId = (int) reader["Id"];
                    var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "Enabled", reader["Enabled"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "ViewContent", reader["ViewContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "ViewOwnContent", reader["ViewOwnContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "PublishContent", reader["PublishContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "PublishOwnContent", reader["PublishOwnContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "EditContent", reader["EditContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "EditOwnContent", reader["EditOwnContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "DeleteContent", reader["DeleteContent"].ToString());
                    contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "DeleteOwnContent", reader["DeleteOwnContent"].ToString());
                });

            if (lastContentItemId == id) {
                // delete the table only when there is no more content to process
                _upgradeService.ExecuteReader("DROP TABLE " + _upgradeService.GetPrefixedTableName("Orchard_ContentPermissions_ContentPermissionsPartRecord"), null);
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateContentMenuItemPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;
            
            _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + _upgradeService.GetPrefixedTableName("Orchard_ContentPicker_ContentMenuItemPartRecord") + " WHERE Id > " + id,
                (reader, connection) => {
                    lastContentItemId = (int)reader["Id"];
                    var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                    contentPermissionPart.As<InfosetPart>().Store("ContentMenuItemPart", "ContentItemId", (int)reader["ContentMenuItemRecord_id"]);
                });

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateTagsPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;
            
            _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Tags_TagsPartRecord") + " WHERE Id > " + id,
                (reader, connection) => {
                    lastContentItemId = (int)reader["Id"];
                    var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                    var tagNames = new List<string>();
                    _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " TR.TagName as TagName FROM "
                                                  + _upgradeService.GetPrefixedTableName("Orchard_Tags_ContentTagRecord") + " as CTR "
                                                  + " INNER JOIN " + _upgradeService.GetPrefixedTableName("Orchard_Tags_TagRecord") + " as TR "
                                                  + " ON CTR.TagRecord_Id = TR.Id"
                                                  + " WHERE TagsPartRecord_id = " + lastContentItemId, (r, c) => tagNames.Add((string) r["TagName"]));


                    contentPermissionPart.As<InfosetPart>().Store("TagsPart", "CurrentTags", String.Join(",", tagNames));
                });

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateWidgetPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Widgets_WidgetPartRecord") + " WHERE Id > " + id,
                (reader, connection) => {
                    lastContentItemId = (int)reader["Id"];
                    var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                    contentPermissionPart.As<InfosetPart>().Store("WidgetPart", "Title", (string)reader["Title"]);
                    contentPermissionPart.As<InfosetPart>().Store("WidgetPart", "Position", (string)reader["Position"]);
                    contentPermissionPart.As<InfosetPart>().Store("WidgetPart", "Zone", (string)reader["Zone"]);
                    contentPermissionPart.As<InfosetPart>().Store("WidgetPart", "RenderTitle", (bool)reader["RenderTitle"]);
                    contentPermissionPart.As<InfosetPart>().Store("WidgetPart", "Name", (string)reader["Name"]);
                });

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateLayerPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + _upgradeService.GetPrefixedTableName("Orchard_Widgets_LayerPartRecord") + " WHERE Id > " + id,
                (reader, connection) => {
                    lastContentItemId = (int)reader["Id"];
                    var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                    contentPermissionPart.As<InfosetPart>().Store("LayerPart", "Name", (string)reader["Name"]);
                    contentPermissionPart.As<InfosetPart>().Store("LayerPart", "Description", (string)reader["Description"]);
                    contentPermissionPart.As<InfosetPart>().Store("LayerPart", "LayerRule", (string)reader["LayerRule"]);
                });

            return new JsonResult { Data = lastContentItemId };
        }
    }
}
