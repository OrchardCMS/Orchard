using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Models;
using Orchard.Data;
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
        private readonly ISignals _signals;
        private readonly IRepository<ContentItemRecord> _contentItemRecord;
        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRecord;

        private const int BATCH = 50;

        public InfosetController(
            IOrchardServices orchardServices,
            IUpgradeService upgradeService,
            IRepository<ContentItemRecord> contentItemRecord,
            IRepository<ContentItemVersionRecord> contentItemVersionRecord,
            ISignals signals) {
            _orchardServices = orchardServices;
            _upgradeService = upgradeService;
            _signals = signals;
            _contentItemVersionRecord = contentItemVersionRecord;
            _contentItemRecord = contentItemRecord;

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
                        site.HomePage = ConvertToString(reader["HomePage"]);
                        site.PageSize = (int)reader["PageSize"];
                        site.PageTitleSeparator = ConvertToString(reader["PageTitleSeparator"]);
                        site.ResourceDebugMode = (ResourceDebugMode)Enum.Parse(typeof(ResourceDebugMode), (string)reader["ResourceDebugMode"]);
                        site.SiteCulture = ConvertToString(reader["SiteCulture"]);
                        site.SiteName = ConvertToString(reader["SiteName"]);
                        site.SiteSalt = ConvertToString(reader["SiteSalt"]);
                        site.SiteTimeZone = ConvertToString(reader["SiteTimeZone"]);
                        site.SuperUser = ConvertToString(reader["SuperUser"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + siteTable, null);

                _signals.Trigger("SiteCurrentTheme");
            }

            #endregion

            #region SiteSettings2PartRecord
            var site2Table = _upgradeService.GetPrefixedTableName("Settings_SiteSettings2PartRecord");
            if (_upgradeService.TableExists(site2Table)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + site2Table,
                    (reader, connection) => {
                        site.BaseUrl = ConvertToString(reader["BaseUrl"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + site2Table, null);
            }

            #endregion

            #region ThemeSiteSettingsPartRecord
            var themesTable = _upgradeService.GetPrefixedTableName("Orchard_Themes_ThemeSiteSettingsPartRecord");
            if (_upgradeService.TableExists(themesTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + themesTable,
                    (reader, connection) => site.As<InfosetPart>().Store("ThemeSiteSettingsPart", "CurrentThemeName", ConvertToString(reader["CurrentThemeName"])));

                _upgradeService.ExecuteReader("DROP TABLE " + themesTable, null);
            }

            #endregion

            #region AkismetSettingsPartRecord
            var akismetTable = _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_AkismetSettingsPartRecord");
            if (_upgradeService.TableExists(akismetTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + akismetTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("AkismetSettingsPart", "TrustAuthenticatedUsers", (bool)reader["TrustAuthenticatedUsers"]);
                        site.As<InfosetPart>().Store("AkismetSettingsPart", "ApiKey", ConvertToString(reader["ApiKey"]));
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + akismetTable, null);
            }

            #endregion

            #region ReCaptchaSettingsPartRecord
            var reCaptchaTable = _upgradeService.GetPrefixedTableName("Orchard_AntiSpam_ReCaptchaSettingsPartRecord");
            if (_upgradeService.TableExists(reCaptchaTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + reCaptchaTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "PublicKey", ConvertToString(reader["PublicKey"]));
                        site.As<InfosetPart>().Store("ReCaptchaSettingsPart", "PrivateKey", ConvertToString(reader["PrivateKey"]));
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
                        site.As<InfosetPart>().Store("TypePadSettingsPart", "ApiKey", ConvertToString(reader["ApiKey"]));
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
                        site.As<InfosetPart>().Store("CacheSettingsPart", "VaryQueryStringParameters", ConvertToString(reader["VaryQueryStringParameters"]));
                        site.As<InfosetPart>().Store("CacheSettingsPart", "VaryRequestHeaders", ConvertToString(reader["VaryRequestHeaders"]));
                        site.As<InfosetPart>().Store("CacheSettingsPart", "IgnoredUrls", ConvertToString(reader["IgnoredUrls"]));
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
                        site.As<InfosetPart>().Store("SearchSettingsPart", "SearchedFields", ConvertToString(reader["SearchedFields"]));
                        site.As<InfosetPart>().Store("SearchSettingsPart", "FilterCulture", (bool)reader["FilterCulture"]);
                        site.As<InfosetPart>().Store("SearchSettingsPart", "SearchIndex", ConvertToString(reader["SearchIndex"]));
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
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "ValidateEmailRegisteredWebsite", ConvertToString(reader["ValidateEmailRegisteredWebsite"]));
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "ValidateEmailContactEmail", ConvertToString(reader["ValidateEmailContactEmail"]));
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "UsersAreModerated", (bool)reader["UsersAreModerated"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "NotifyModeration", (bool)reader["NotifyModeration"]);
                        site.As<InfosetPart>().Store("RegistrationSettingsPart", "NotificationsRecipients", ConvertToBool(reader["NotificationsRecipients"]));
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
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Address", ConvertToString(reader["Address"]));
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Host", ConvertToString(reader["Host"]));
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Port", (int)reader["Port"]);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "EnableSsl", (bool)reader["EnableSsl"]);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "RequireCredentials", (bool)reader["RequireCredentials"]);
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "UserName", ConvertToString(reader["UserName"]));
                        site.As<InfosetPart>().Store("SmtpSettingsPart", "Password", ConvertToString(reader["Password"]));
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + emailTable, null);
            }

            #endregion

            #region WarmupSettingsPartRecord
            var warmupTable = _upgradeService.GetPrefixedTableName("Orchard_Warmup_WarmupSettingsPartRecord");
            if (_upgradeService.TableExists(warmupTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + warmupTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "Urls", ConvertToString(reader["Urls"]));
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "Scheduled", (bool)reader["Scheduled"]);
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "Delay", (int)reader["Delay"]);
                        site.As<InfosetPart>().Store("WarmupSettingsPart", "OnPublish", (bool)reader["OnPublish"]);
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + warmupTable, null);
            }

            #endregion

            #region BlogPartRecord
            var blogTable = _upgradeService.GetPrefixedTableName("Orchard_Blogs_BlogPartRecord");
            if (_upgradeService.TableExists(blogTable)) {
                _upgradeService.ExecuteReader("SELECT * FROM " + blogTable,
                    (reader, connection) => {
                        site.As<InfosetPart>().Store("BlogPart", "Description", ConvertToString(reader["Description"]));
                    });

                _upgradeService.ExecuteReader("DROP TABLE " + blogTable, null);
            }
            #endregion

            _orchardServices.Notifier.Success(T("Site Settings migrated successfully"));

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
                lastContentItemId = part.Record.Id;
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
                lastContentItemId = part.Record.Id;
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

                        if (contentPermissionPart != null) {
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "Enabled", (bool)reader["Enabled"]);
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "ViewContent", ConvertToString(reader["ViewContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "ViewOwnContent", ConvertToString(reader["ViewOwnContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "PublishContent", ConvertToString(reader["PublishContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "PublishOwnContent", ConvertToString(reader["PublishOwnContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "EditContent", ConvertToString(reader["EditContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "EditOwnContent", ConvertToString(reader["EditOwnContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "DeleteContent", ConvertToString(reader["DeleteContent"]));
                            contentPermissionPart.As<InfosetPart>().Store("ContentPermissionsPart", "DeleteOwnContent", ConvertToString(reader["DeleteOwnContent"])); 
                        }
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
                        var contentMenuItemPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (contentMenuItemPart != null) {
                            contentMenuItemPart.As<InfosetPart>().Store("ContentMenuItemPart", "ContentItemId", (int)reader["ContentMenuItemRecord_id"]); 
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
                        var tagsPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (tagsPart != null) {
                            var tagNames = new List<string>();
                            _upgradeService.ExecuteReader("SELECT TOP " + BATCH + " TR.TagName as TagName FROM "
                                                            + _upgradeService.GetPrefixedTableName("Orchard_Tags_ContentTagRecord") + " as CTR "
                                                            + " INNER JOIN " + _upgradeService.GetPrefixedTableName("Orchard_Tags_TagRecord") + " as TR "
                                                            + " ON CTR.TagRecord_Id = TR.Id"
                                                            + " WHERE TagsPartRecord_id = " + lastContentItemId, (r, c) => tagNames.Add((string)r["TagName"]));


                            tagsPart.As<InfosetPart>().Store("TagsPart", "CurrentTags", String.Join(",", tagNames)); 
                        }
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
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Title", ConvertToString(reader["Title"]));
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Position", ConvertToString(reader["Position"]));
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Zone", ConvertToString(reader["Zone"]));
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "RenderTitle", (bool)reader["RenderTitle"]);
                            widgetPart.As<InfosetPart>().Store("WidgetPart", "Name", ConvertToString(reader["Name"]));
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
                        var layerPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (layerPart != null) {
                            layerPart.As<InfosetPart>().Store("LayerPart", "Name", ConvertToString(reader["Name"]));
                            layerPart.As<InfosetPart>().Store("LayerPart", "Description", ConvertToString(reader["Description"]));
                            layerPart.As<InfosetPart>().Store("LayerPart", "LayerRule", ConvertToString(reader["LayerRule"])); 
                        }
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
                        var menuWidgetPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (menuWidgetPart != null) {
                            menuWidgetPart.As<InfosetPart>().Store("MenuWidgetPart", "StartLevel", (int)reader["StartLevel"]);
                            menuWidgetPart.As<InfosetPart>().Store("MenuWidgetPart", "Levels", (int)reader["Levels"]);
                            menuWidgetPart.As<InfosetPart>().Store("MenuWidgetPart", "Breadcrumb", (bool)reader["Breadcrumb"]);
                            menuWidgetPart.As<InfosetPart>().Store("MenuWidgetPart", "AddHomePage", (bool)reader["AddHomePage"]);
                            menuWidgetPart.As<InfosetPart>().Store("MenuWidgetPart", "AddCurrentPage", (bool)reader["AddCurrentPage"]);
                            menuWidgetPart.As<InfosetPart>().Store("MenuWidgetPart", "MenuContentItemId", (int)reader["Menu_id"]); 
                        }
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

                        contentPermissionPart.As<InfosetPart>().Store("ShapeMenuItemPart", "ShapeType", ConvertToString(reader["ShapeType"]));
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
                        var menuItemPart = _orchardServices.ContentManager.Get(lastContentItemId);

                        if (menuItemPart != null) {
                            menuItemPart.As<InfosetPart>().Store("MenuItemPart", "Url", ConvertToString(reader["Url"])); 
                        }
                    });

                if (lastContentItemId == id) {
                    // delete the table only when there is no more content to process
                    _upgradeService.ExecuteReader("DROP TABLE " + menuItemTable, null);
                }
            }

            return new JsonResult { Data = lastContentItemId };
        }

        [HttpPost]
        public JsonResult FixContentItemVersionPart(int id) {
            string[] ignoredParts = new string[] { "CommonPart", "WidgetPart", "LayerPart", "IdentityPart", "UserPart", "MenuItemPart"};

            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            var lastContentItemVersionId = id;

            var contentItemVersionTable = _upgradeService.GetPrefixedTableName("Orchard_Framework_ContentItemVersionRecord");
            var contentItemTable = _upgradeService.GetPrefixedTableName("Orchard_Framework_ContentItemRecord");
            
            foreach(var contentItemRecord in _contentItemRecord.Table.Take(BATCH).Where(x => x.Id > id)) {
                    lastContentItemVersionId = contentItemRecord.Id;
                    if (!String.IsNullOrWhiteSpace(contentItemRecord.Data)) {
                        var data = XDocument.Parse(contentItemRecord.Data).Root; // <Data /> element

                        foreach (var contentItemVersionRecord in _contentItemVersionRecord.Table.Where(x => (x.Published || x.Latest) && x.ContentItemRecord == contentItemRecord)) {
                            var versionData = new XDocument(new XElement("Data")).Root;
                            if (!String.IsNullOrWhiteSpace(contentItemVersionRecord.Data)) {
                                versionData = XDocument.Parse(contentItemVersionRecord.Data).Root;
                            }

                            // copy each XML element from ContentItem to ContentItemVersionRecord
                            foreach (XElement element in data.Elements()) {
                                
                                if (ignoredParts.Contains(element.Name.ToString())) {
                                    continue;
                                }

                                if (element.Name.ToString().EndsWith("SettingsPart")) {
                                    continue;
                                }

                                var versionElement = versionData.Element(element.Name);
                                if (versionElement != null) {
                                    versionElement.Remove();
                                }
                                
                                versionData.Add(element);
                            }
                            
                            contentItemVersionRecord.Data = versionData.ToString(SaveOptions.DisableFormatting);
                        }
                    }
            }

            return new JsonResult { Data = lastContentItemVersionId };
        }

        private static string ConvertToString(object readerValue) {
            return readerValue == DBNull.Value ? null : (string)readerValue;
        }

        private static bool ConvertToBool(object readerValue) {
            if (readerValue == null) {
                return false;
            }

            var stringRepresentation = readerValue.ToString();

            if (String.IsNullOrEmpty(stringRepresentation)) {
                return false;
            }

            bool result;
            if (bool.TryParse(stringRepresentation, out result)) {
                return result;
            }
            return false;
        }

        public object SiteThemeService { get; set; }
    }
}
