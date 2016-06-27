using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.Azure.MediaServices.ViewModels.Settings;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Azure.MediaServices.Controllers {

    [Admin]
    public class SettingsController : Controller {
        private readonly IOrchardServices _services;
        private readonly IWamsClient _wamsClient;

        public SettingsController(IOrchardServices services, IWamsClient wamsClient) {
            _services = services;
            _wamsClient = wamsClient;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            if (!_services.Authorizer.Authorize(Permissions.ManageCloudMediaSettings, T("You are not authorized to manage Microsoft Azure Media settings.")))
                return new HttpUnauthorizedResult();

            var settings = _services.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();
            var viewModel = new SettingsViewModel {
                General = new GeneralSettingsViewModel {
                    WamsAccountName = settings.WamsAccountName,
                    WamsAccountKey = settings.WamsAccountKey,
                    StorageAccountKey = settings.StorageAccountKey,
                    EnableDynamicPackaging = settings.EnableDynamicPackaging,
                    AccessPolicyDuration = settings.AccessPolicyDuration,
                    AllowedVideoFilenameExtensions = String.Join(";", settings.AllowedVideoFilenameExtensions)
                },
                EncodingSettings = new EncodingSettingsViewModel {
                    WamsEncodingPresets = settings.WamsEncodingPresets,
                    DefaultWamsEncodingPresetIndex = settings.DefaultWamsEncodingPresetIndex
                },
                EncryptionSettings = new EncryptionSettingsViewModel {
                    KeySeedValue = settings.EncryptionKeySeedValue,
                    LicenseAcquisitionUrl = settings.EncryptionLicenseAcquisitionUrl,
                },
                SubtitleLanguages = new SubtitleLanguagesSettingsViewModel {
                    Languages = settings.SubtitleLanguages
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Save(SettingsViewModel viewModel) {
            if (!_services.Authorizer.Authorize(Permissions.ManageCloudMediaSettings, T("You are not authorized to manage Microsoft Azure Media settings.")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                return View("Index", viewModel);
            }

            var presetPattern = new Regex(@"^[\w\s]*$");
            foreach (var preset in viewModel.EncodingSettings.WamsEncodingPresets) {
                if (!presetPattern.IsMatch(preset.Name)) {
                    _services.Notifier.Error(T("The encoding preset name '{0}' is invalid. Encoding presets can only contain letters, numbers and spaces.", preset.Name));
                    return View("Index", viewModel);
                }
            }

            Logger.Debug("User requested to save module settings.");

            var settings = _services.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();

            if (!String.IsNullOrWhiteSpace(viewModel.General.WamsAccountName) && !String.IsNullOrEmpty(viewModel.General.WamsAccountKey)) {
                // Test WAMS credentials if they were changed.
                if (viewModel.General.WamsAccountName != settings.WamsAccountName || viewModel.General.WamsAccountKey != settings.WamsAccountKey || viewModel.General.StorageAccountKey != settings.StorageAccountKey) {
                    if (!TestCredentialsInternal(viewModel.General.WamsAccountName, viewModel.General.WamsAccountKey, viewModel.General.StorageAccountKey)) {
                        _services.Notifier.Error(T("The account credentials verification failed. The settings were not saved."));
                        return View("Index", viewModel);
                    }
                    else {
                        _services.Notifier.Information(T("The new account credentials were successfully verified."));
                    }
                }
            }

            var previousStorageAccountKey = settings.StorageAccountKey;

            settings.WamsAccountName = viewModel.General.WamsAccountName.TrimSafe();
            settings.WamsAccountKey = viewModel.General.WamsAccountKey.TrimSafe();
            settings.StorageAccountKey = viewModel.General.StorageAccountKey.TrimSafe();
            settings.EnableDynamicPackaging = viewModel.General.EnableDynamicPackaging;
            settings.AccessPolicyDuration = viewModel.General.AccessPolicyDuration;
            settings.AllowedVideoFilenameExtensions = viewModel.General.AllowedVideoFilenameExtensions.Split(';');
            settings.WamsEncodingPresets = viewModel.EncodingSettings.WamsEncodingPresets;
            settings.DefaultWamsEncodingPresetIndex = viewModel.EncodingSettings.DefaultWamsEncodingPresetIndex;
            settings.SubtitleLanguages = viewModel.SubtitleLanguages != null ? viewModel.SubtitleLanguages.Languages : null;

            // TODO: Encryption is disabled for now. Uncomment when we need it again.
            //settings.EncryptionKeySeedValue = viewModel.EncryptionSettings.KeySeedValue.TrimSafe();
            //settings.EncryptionLicenseAcquisitionUrl = viewModel.EncryptionSettings.LicenseAcquisitionUrl.TrimSafe();

            // Configure storage account for CORS if account key was specified and changed.
            if (settings.IsValid() && !String.IsNullOrWhiteSpace(settings.StorageAccountKey)) {
                if (settings.StorageAccountKey != previousStorageAccountKey) {
                    try {
                        Logger.Debug("Ensuring CORS support for the configured base URL and the current request URL.");
                        var originsToAdd = new List<string>();
                        var baseUrlOrigin = new Uri(_services.WorkContext.CurrentSite.BaseUrl).GetLeftPart(UriPartial.Authority);
                        originsToAdd.Add(baseUrlOrigin);

                        var currentUrlOrigin = _services.WorkContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                        if (!originsToAdd.Contains(currentUrlOrigin))
                            originsToAdd.Add(currentUrlOrigin);

                        var addedOrigins = _wamsClient.EnsureCorsIsEnabledAsync(originsToAdd.ToArray()).Result;

                        if (addedOrigins.Any()) {
                            Logger.Information("CORS rules were added to the configured storage account for the following URLs: {0}.", String.Join("; ", addedOrigins));
                            _services.Notifier.Information(T("CORS rules have been configured on your storage account for the following URLs: {0}.", String.Join("; ", addedOrigins)));
                        }
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "Error while ensuring CORS support.");
                        _services.Notifier.Warning(T("Failed to check or configure CORS support on your storage account."));
                    }
                }
            }

            Logger.Information("Module settings were saved.");
            _services.Notifier.Information(T("The settings were saved successfully."));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TestCredentials(SettingsViewModel viewModel) {
            if (!_services.Authorizer.Authorize(Permissions.ManageCloudMediaSettings, T("You are not authorized to manage Microsoft Azure Media settings.")))
                return new HttpUnauthorizedResult();

            Logger.Debug("User requested to verify WAMS account credentials.");

            if (TestCredentialsInternal(viewModel.General.WamsAccountName, viewModel.General.WamsAccountKey, viewModel.General.StorageAccountKey)) {
                _services.Notifier.Information(T("The account credentials were successfully verified."));
            }
            else {
                _services.Notifier.Error(T("The account credentials verification failed."));
            }

            return View("Index", viewModel);
        }

        private bool TestCredentialsInternal(string wamsAccountName, string wamsAccountKey, string storageAccountKey) {
            try {
                // This will trigger an authentication call to WAMS.
                var ctx = new CloudMediaContext(wamsAccountName, wamsAccountKey);

                if (!String.IsNullOrWhiteSpace(storageAccountKey)) {
                    // This will trigger an authentication call to Microsoft Azure Storage.
                    var storageAccount = new CloudStorageAccount(new StorageCredentials(ctx.DefaultStorageAccount.Name, storageAccountKey), false);
                    storageAccount.CreateCloudBlobClient().GetServiceProperties();
                    Logger.Information("Storage account credentials were verified.");
                }

                Logger.Information("WAMS account credentials were verified.");
                return true;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error while verifying WAMS and storage account credentials.");
                return false;
            }
        }
    }
}