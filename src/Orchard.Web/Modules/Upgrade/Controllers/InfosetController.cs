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
using Orchard.MediaLibrary.Models;
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

            #region SiteSettingsPartRecord
            var siteTable = _upgradeService.GetPrefixedTableName("Settings_SiteSettingsPartRecord");
            if (_upgradeService.TableExists(siteTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + siteTable,
                    (reader, connection) => {
                        site.HomePage = reader["HomePage"] as string;
                        site.PageSize = (int)reader["PageSize"];
                        site.PageTitleSeparator = (string)reader["PageTitleSeparator"];
                        site.ResourceDebugMode = (ResourceDebugMode)Enum.Parse(typeof(ResourceDebugMode), (string)reader["ResourceDebugMode"]);
                        site.SiteCulture = (string)reader["SiteCulture"];
                        site.SiteName = (string)reader["SiteName"];
                        site.SiteSalt = (string)reader["SiteSalt"];
                        site.SiteTimeZone = (string)reader["SiteTimeZone"];
                        site.SuperUser = (string)reader["SuperUser"];
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + siteTable, null);
            }

            #endregion

            #region SiteSettings2PartRecord
            var site2Table = _upgradeService.GetPrefixedTableName("Settings_SiteSettings2PartRecord");
            if (_upgradeService.TableExists(site2Table)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + site2Table,
                    (reader, connection) => {
                        site.BaseUrl = (string)reader["BaseUrl"];
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + site2Table, null);
            }

            #endregion

            #region ThemeSiteSettingsPartRecord
            var themesTable = _upgradeService.GetPrefixedTableName("Orchard_Themes_ThemeSiteSettingsPartRecord");
            if (_upgradeService.TableExists(themesTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + themesTable,
                    (reader, connection) => site.As<InfosetPart>().Store("ThemeSiteSettingsPart", "CurrentThemeName", (string)reader["CurrentThemeName"]));

                _upgradeService.ExecuteReader("DROP TABLE " + themesTable, null);
            }

            #endregion

            #region AkismetSettingsPartRecord
            var akismetTable = _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_AkismetSettingsPartRecord");
            if (_upgradeService.TableExists(akismetTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + akismetTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("AkismetSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                        site.As<InfosetPart>().Store("AkismetSettingsPart", "ApiKey", reader["ApiKey"].ToString());
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + akismetTable, null);
            }

            #endregion

            #region ReCaptchaSettingsPartRecord
            var reCaptchaTable = _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_ReCaptchaSettingsPartRecord");
            if (_upgradeService.TableExists(reCaptchaTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + reCaptchaTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "PublicKey", reader["PublicKey"].ToString());
                        site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "PrivateKey", reader["PrivateKey"].ToString());
                        site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + reCaptchaTable, null);
            }

            #endregion

            #region TypePadSettingsPartRecord
            var typePadTable = _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_TypePadSettingsPartRecord");
            if (_upgradeService.TableExists(typePadTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + typePadTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("TypePadSettingsPart", "ApiKey", reader["ApiKey"].ToString());
                        site.As<InfosetPart>().Store("TypePadSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + typePadTable, null);
            }

            #endregion

            #region CacheSettingsPartRecord
            var cacheTable = _upgradeService.GetPrefixedTableName("Orchard_OutputCache_CacheSettingsPartRecord");
            if (_upgradeService.TableExists(cacheTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + cacheTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("CacheSettingsPart", "DefaultCacheDuration", (int)reader["DefaultCacheDuration"]);
                        site.As<InfosetPart>().Store("CacheSettingsPart", "DefaultMaxAge", (int)reader["DefaultMaxAge"]);
                        site.As<InfosetPart>().Store("CacheSettingsPart", "VaryQueryStringParameters", reader["VaryQueryStringParameters"] as string);
                        site.As<InfosetPart>().Store("CacheSettingsPart", "VaryRequestHeaders", reader["VaryRequestHeaders"] as string);
                        site.As<InfosetPart>().Store("CacheSettingsPart", "IgnoredUrls", reader["IgnoredUrls"] as string);
                        site.As<InfosetPart>().Store("CacheSettingsPart", "ApplyCulture", (bool)reader["ApplyCulture"]);
                        site.As<InfosetPart>().Store("CacheSettingsPart", "DebugMode", (bool)reader["DebugMode"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + cacheTable, null);
            }

            #endregion

            #region CommentSettingsPartRecord
            var commentsTable = _upgradeService.GetPrefixedTableName("Orchard_Comments_CommentSettingsPartRecord");
            if (_upgradeService.TableExists(commentsTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + commentsTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("CommentSettingsPart", "ModerateComments", (bool)reader["ModerateComments"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + commentsTable, null);
            }

            #endregion

            #region  MessageSettingsPartRecord
            var messagesTable = _upgradeService.GetPrefixedTableName("Orchard_Messaging_MessageSettingsPartRecord");
            if (_upgradeService.TableExists(messagesTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + messagesTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("MessageSettingsPart", "DefaultChannelService", reader["DefaultChannelService"] as string);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + messagesTable, null);
            }

            #endregion

            #region SearchSettingsPartRecord
            var searchTable = _upgradeService.GetPrefixedTableName("Orchard_Search_SearchSettingsPartRecord");
            if (_upgradeService.TableExists(searchTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + searchTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("SearchSettingsPart", "SearchedFields", reader["SearchedFields"] as string);
                        site.As<InfosetPart>().Store("SearchSettingsPart", "FilterCulture", (bool)reader["FilterCulture"]);
                        site.As<InfosetPart>().Store("SearchSettingsPart", "SearchIndex", reader["SearchIndex"] as string);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + searchTable, null);
            }

            #endregion

            #region RegistrationSettingsPartRecord
            var registrationTable = _upgradeService.GetPrefixedTableName("Orchard_Users_RegistrationSettingsPartRecord");
            if (_upgradeService.TableExists(registrationTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + registrationTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "UsersCanRegister", (bool)reader["UsersCanRegister"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "UsersMustValidateEmail", (bool)reader["UsersMustValidateEmail"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "UsersCanRegister", (bool)reader["UsersCanRegister"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "ValidateEmailRegisteredWebsite", SafelyConvertFieldToBool(reader["ValidateEmailRegisteredWebsite"]));
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "ValidateEmailContactEMail", SafelyConvertFieldToBool(reader["ValidateEmailContactEMail"]));
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "UsersAreModerated", (bool)reader["UsersAreModerated"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "NotifyModeration", (bool)reader["NotifyModeration"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "NotificationsRecipients", SafelyConvertFieldToBool(reader["NotificationsRecipients"]));
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "EnableLostPassword", (bool)reader["EnableLostPassword"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + registrationTable, null);
            }

            #endregion

            #region SmtpSettingsPartRecord
            var emailTable = _upgradeService.GetPrefixedTableName("Orchard_Email_SmtpSettingsPartRecord");
            if (_upgradeService.TableExists(emailTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + emailTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Address", reader["Address"] as string);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Host", reader["Host"] as string);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Port", (int)reader["Port"]);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "EnableSsl", (bool)reader["EnableSsl"]);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "RequireCredentials", (bool)reader["RequireCredentials"]);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "UserName", reader["UserName"] as string);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Password", reader["Password"] as string);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + emailTable, null);
            }

            #endregion

            #region WarmupSettingsPartRecord
            var warmupTable = _upgradeService.GetPrefixedTableName("Orchard_Warmup_WarmupSettingsPartRecord");
            if (_upgradeService.TableExists(warmupTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + warmupTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "Urls", reader["Urls"] as string);
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "Scheduled", (bool)reader["Scheduled"]);
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "Delay", (int)reader["Delay"]);
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "OnPublish", (bool)reader["OnPublish"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + warmupTable, null);
            }

            #endregion

            // todo: user records

            _orchardServices.Notifier.Information(T("Site Settings migrated successfully"));

            return View();
        }

        [HttpPost]
        public JsonResult MigrateBody(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var parts = _orchardServices.ContentManager
                .Query<BodyPart, BodyPartRecord>()
                .Where(x => x.Id > id)
                .OrderBy(x => x.Id)
                .Slice(0, BATCH).ToList();

            var lastContentItemId = id;

            foreach (var part in parts) {
                part.Text = part.Text;
                lastContentItemId = part.Id;
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateMedia(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var parts = _orchardServices.ContentManager
                .Query<MediaPart, MediaPartRecord>()
                .Where(x => x.Id > id)
                .OrderBy(x => x.Id)
                .Slice(0, BATCH).ToList();

            var lastContentItemId = id;

            foreach (var part in parts) {
                part.MimeType = part.MimeType;
                part.Caption = part.Caption;
                part.AlternateText = part.AlternateText;
                part.FolderPath = part.FolderPath;
                part.FileName = part.FileName;
                lastContentItemId = part.Id;
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateContentPermissionsPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var permissionsTable = _upgradeService.GetPrefixedTableName("Orchard_ContentPermissions_ContentPermissionsPartRecord");
            if (_upgradeService.TableExists(permissionsTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + permissionsTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
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
                    _upgradeService.ExecuteReader("DROP TABLE " + permissionsTable, null);
                }
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateContentMenuItemPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var contentMenuItemTable = _upgradeService.GetPrefixedTableName("Orchard_ContentPicker_ContentMenuItemPartRecord");
            if (_upgradeService.TableExists(contentMenuItemTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + contentMenuItemTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (contentPermissionPart != null) {
                            contentPermissionPart.As<InfosetPart>().Store("ContentMenuItemPart", "ContentItemId", (int)reader["ContentMenuItemRecord_id"]); 
                        }
                    });
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateTagsPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var tagsTable = _upgradeService.GetPrefixedTableName("Orchard_Tags_TagsPartRecord");
            if (_upgradeService.TableExists(tagsTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + tagsTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        var tagNames = new List<string>();
                        _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " TR.TagName as TagName FROM "
                                                        + _upgradeService.GetPrefixedTableName("Orchard_Tags_ContentTagRecord") + " as CTR "
                                                        + " INNER JOIN " + _upgradeService.GetPrefixedTableName("Orchard_Tags_TagRecord") + " as TR "
                                                        + " ON CTR.TagRecord_Id = TR.Id"
                                                        + " WHERE TagsPartRecord_id = " + lastContentItemId, (r, c) => tagNames.Add((string)r["TagName"]));


                        contentPermissionPart.As<InfosetPart>().Store("TagsPart", "CurrentTags", String.Join(",", tagNames));
                    });
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateWidgetPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var widgetsTable = _upgradeService.GetPrefixedTableName("Orchard_Widgets_WidgetPartRecord");
            if (_upgradeService.TableExists(widgetsTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + widgetsTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var widgetPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (widgetPart != null) {
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Title", (string)reader["Title"]);
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Position", (string)reader["Position"]);
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Zone", (string)reader["Zone"]);
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "RenderTitle", (bool)reader["RenderTitle"]);
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Name", reader["Name"] as string);
                        }
                    });
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateLayerPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var layersTable = _upgradeService.GetPrefixedTableName("Orchard_Widgets_LayerPartRecord");
            if (_upgradeService.TableExists(layersTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + layersTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        contentPermissionPart.As<InfosetPart>().Store("LayerPart", "Name", (string)reader["Name"]);
                        contentPermissionPart.As<InfosetPart>().Store("LayerPart", "Description", (string)reader["Description"]);
                        contentPermissionPart.As<InfosetPart>().Store("LayerPart", "LayerRule", (string)reader["LayerRule"]);
                    });
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateMenuWidgetPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var menuWidgetTable = _upgradeService.GetPrefixedTableName("Navigation_MenuWidgetPartRecord");
            if (_upgradeService.TableExists(menuWidgetTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + menuWidgetTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        contentPermissionPart.As<InfosetPart>().Store("MenuWidgetPart", "StartLevel", (int)reader["StartLevel"]);
                        contentPermissionPart.As<InfosetPart>().Store("MenuWidgetPart", "Levels", (int)reader["Levels"]);
                        contentPermissionPart.As<InfosetPart>().Store("MenuWidgetPart", "Breadcrumb", (bool)reader["Breadcrumb"]);
                        contentPermissionPart.As<InfosetPart>().Store("MenuWidgetPart", "AddHomePage", (bool)reader["AddHomePage"]);
                        contentPermissionPart.As<InfosetPart>().Store("MenuWidgetPart", "AddCurrentPage", (bool)reader["AddCurrentPage"]);
                        contentPermissionPart.As<InfosetPart>().Store("MenuWidgetPart", "MenuContentItemId", (int)reader["Menu_id"]);
                    });

                if (lastContentItemId == id) {
                    // delete the table only when there is no more content to process
                    _upgradeService.ExecuteReader("DROP TABLE " + menuWidgetTable, null);
                }
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult MigrateShapeMenuItemPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var shapeMenuItemTable = _upgradeService.GetPrefixedTableName("Navigation_ShapeMenuItemPartRecord");
            if (_upgradeService.TableExists(shapeMenuItemTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + shapeMenuItemTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        contentPermissionPart.As<InfosetPart>().Store("ShapeMenuItemPart", "ShapeType", (string)reader["ShapeType"]);
                    });

                if (lastContentItemId == id) {
                    // delete the table only when there is no more content to process
                    _upgradeService.ExecuteReader("DROP TABLE " + shapeMenuItemTable, null);
                }
            }

            return new JsonResult { Data = lastContentItemId };
        }


        [HttpPost]
        public JsonResult MigrateMenuItemPart(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemId = id;

            var menuItemTable = _upgradeService.GetPrefixedTableName("Navigation_MenuItemPartRecord");
            if (_upgradeService.TableExists(menuItemTable)) {
                _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " * FROM " + menuItemTable + " WHERE Id > " + id,
                    (reader, connection) => {
                        lastContentItemId = (int)reader["Id"];
                        var contentPermissionPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        contentPermissionPart.As<InfosetPart>().Store("MenuItemPart", "Url", reader["Url"] as string);
                    });

                if (lastContentItemId == id) {
                    // delete the table only when there is no more content to process
                    _upgradeService.ExecuteReader("DROP TABLE " + menuItemTable, null);
                }
            }

            return new JsonResult { Data = lastContentItemId };
        }


        private static bool SafelyConvertFieldToBool(object field) {
            if (field == null) {
                return false;
            }

            var stringRepresentation = field.ToString();

            if (String.IsNullOrEmpty(stringRepresentation)) {
                return false;
            }

            bool result;
            if (bool.TryParse(stringRepresentation, out result)) {
                return result;
            }
            return false;
        }
    }
}
