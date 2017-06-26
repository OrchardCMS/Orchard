using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Data.Migration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes.Events;
using Orchard.Themes.Models;
using Orchard.Themes.Preview;
using Orchard.Themes.Services;
using Orchard.Themes.ViewModels;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using Orchard.Environment.Configuration;

namespace Orchard.Themes.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IExtensionDisplayEventHandler _extensionDisplayEventHandler;
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IFeatureManager _featureManager;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IPreviewTheme _previewTheme;
        private readonly IThemeService _themeService;
        private readonly ShellSettings _shellSettings;

        private const string AlreadyEnabledFeatures = "Orchard.Themes.AlreadyEnabledFeatures";

        public AdminController(
            IEnumerable<IExtensionDisplayEventHandler> extensionDisplayEventHandlers,
            IOrchardServices services,
            IDataMigrationManager dataMigraitonManager,
            IFeatureManager featureManager,
            ISiteThemeService siteThemeService,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IPreviewTheme previewTheme, 
            IThemeService themeService,
            ShellSettings shellSettings) {
            Services = services;

            _extensionDisplayEventHandler = extensionDisplayEventHandlers.FirstOrDefault();
            _dataMigrationManager = dataMigraitonManager;
            _siteThemeService = siteThemeService;
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _featureManager = featureManager;
            _previewTheme = previewTheme;
            _themeService = themeService;
            _shellSettings = shellSettings;
            
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            bool installThemes = 
                _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null 
                && Services.Authorizer.Authorize(StandardPermissions.SiteOwner) // only site owners
                && _shellSettings.Name == ShellSettings.DefaultName; // of the default tenant

            var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            ThemeEntry currentTheme = null;
            ExtensionDescriptor currentThemeDescriptor = _siteThemeService.GetSiteTheme();
            if (currentThemeDescriptor != null) {
                currentTheme = new ThemeEntry(currentThemeDescriptor);
            }

            IEnumerable<ThemeEntry> themes = _extensionManager.AvailableExtensions()
                .Where(extensionDescriptor => {
                        bool hidden = false;
                        string tags = extensionDescriptor.Tags;
                        if (tags != null) {
                            hidden = tags.Split(',').Any(t => t.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase));
                        }

                        // is the theme allowed for this tenant ?
                        bool allowed = _shellSettings.Themes.Length == 0 || _shellSettings.Themes.Contains(extensionDescriptor.Id);

                        return !hidden && allowed &&
                                DefaultExtensionTypes.IsTheme(extensionDescriptor.ExtensionType) &&
                                (currentTheme == null ||
                                !currentTheme.Descriptor.Id.Equals(extensionDescriptor.Id));
                    })
                .Select(extensionDescriptor => {
                        ThemeEntry themeEntry = new ThemeEntry(extensionDescriptor) {
                            NeedsUpdate = featuresThatNeedUpdate.Contains(extensionDescriptor.Id),
                            IsRecentlyInstalled = _themeService.IsRecentlyInstalled(extensionDescriptor),
                            Enabled = _shellDescriptor.Features.Any(sf => sf.Name == extensionDescriptor.Id),
                            CanUninstall = installThemes
                        };

                        if (_extensionDisplayEventHandler != null) {
                            foreach (string notification in _extensionDisplayEventHandler.Displaying(themeEntry.Descriptor, ControllerContext.RequestContext))
                            {
                                themeEntry.Notifications.Add(notification);
                            }
                        }

                        return themeEntry;
                    })
                .ToArray();

            return View(new ThemesIndexViewModel {
                CurrentTheme = currentTheme,
                InstallThemes = installThemes,
                Themes = themes
            });
        }

        [HttpPost, FormValueAbsent("submit.Apply"), FormValueAbsent("submit.Cancel")]
        public ActionResult Preview(string themeId, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                return new HttpUnauthorizedResult();

            if (_extensionManager.AvailableExtensions()
                .FirstOrDefault(extension => DefaultExtensionTypes.IsTheme(extension.ExtensionType) && extension.Id.Equals(themeId)) == null) {

                Services.Notifier.Error(T("Theme {0} was not found", themeId));
            } else {
                var alreadyEnabledFeatures = GetEnabledFeatures();
                _themeService.EnableThemeFeatures(themeId);
                _previewTheme.SetPreviewTheme(themeId);
                alreadyEnabledFeatures.Except(new[] { themeId });
                TempData[AlreadyEnabledFeatures] = alreadyEnabledFeatures;
            }

            return this.RedirectLocal(returnUrl, "~/");
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Apply")]
        public ActionResult ApplyPreview(string themeId, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                return new HttpUnauthorizedResult();

            if (_extensionManager.AvailableExtensions()
                .FirstOrDefault(extension => DefaultExtensionTypes.IsTheme(extension.ExtensionType) && extension.Id.Equals(themeId)) == null) {

                Services.Notifier.Error(T("Theme {0} was not found", themeId));
            } else {
                _previewTheme.SetPreviewTheme(null);
                _siteThemeService.SetSiteTheme(themeId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Cancel")]
        public ActionResult CancelPreview(string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                return new HttpUnauthorizedResult();

            if (TempData.ContainsKey(AlreadyEnabledFeatures)) {

                var alreadyEnabledFeatures = TempData[AlreadyEnabledFeatures] as IEnumerable<string>;
                if (alreadyEnabledFeatures != null) {
                    var afterEnabledFeatures = GetEnabledFeatures();
                    if (afterEnabledFeatures.Count() > alreadyEnabledFeatures.Count()) {
                        var disableFeatures = afterEnabledFeatures.Except(alreadyEnabledFeatures);
                        _themeService.DisablePreviewFeatures(disableFeatures);
                    }
                }
            }

            _previewTheme.SetPreviewTheme(null);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(string themeId) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't enable the theme")))
                return new HttpUnauthorizedResult();

            if (_extensionManager.AvailableExtensions()
                .FirstOrDefault(extension => DefaultExtensionTypes.IsTheme(extension.ExtensionType) && extension.Id.Equals(themeId)) == null) {

                Services.Notifier.Error(T("Theme {0} was not found", themeId));
            } else {
                _themeService.EnableThemeFeatures(themeId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(string themeId) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't disable the current theme")))
                return new HttpUnauthorizedResult();

            if (_extensionManager.AvailableExtensions()
                .FirstOrDefault(extension => DefaultExtensionTypes.IsTheme(extension.ExtensionType) && extension.Id.Equals(themeId)) == null) {

                Services.Notifier.Error(T("Theme {0} was not found", themeId));
            } else {
                _themeService.DisableThemeFeatures(themeId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Activate(string themeId) {
            if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't set the current theme")))
                return new HttpUnauthorizedResult();

            if (_extensionManager.AvailableExtensions()
                .FirstOrDefault(extension => DefaultExtensionTypes.IsTheme(extension.ExtensionType) && extension.Id.Equals(themeId)) == null) {

                Services.Notifier.Error(T("Theme {0} was not found", themeId));
            } 
            else if (_shellSettings.Themes.Any() && !_shellSettings.Themes.Contains(themeId)) {
                return new HttpUnauthorizedResult();
            }
            else {
                _themeService.EnableThemeFeatures(themeId);
                _siteThemeService.SetSiteTheme(themeId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(string themeId) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Couldn't update theme")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(themeId))
                return HttpNotFound();

            try {
                _dataMigrationManager.Update(themeId);
                Services.Notifier.Information(T("The theme {0} was updated successfully.", themeId));
                Logger.Information("The theme {0} was updated successfully.", themeId);
            } catch (Exception exception) {
                Logger.Error(T("An error occurred while updating the theme {0}: {1}", themeId, exception.Message).Text);
                Services.Notifier.Error(T("An error occurred while updating the theme {0}: {1}", themeId, exception.Message));
            }

            return RedirectToAction("Index");
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class FormValueAbsentAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueAbsentAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return string.IsNullOrEmpty(value);
            }
        }

        public IEnumerable<string> GetEnabledFeatures() {
            return _featureManager.GetEnabledFeatures().Select(f => f.Id);
        }
    }
}