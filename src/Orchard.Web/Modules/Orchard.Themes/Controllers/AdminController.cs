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
using Orchard.Mvc.Extensions;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.Themes.Events;
using Orchard.Themes.Models;
using Orchard.Themes.Preview;
using Orchard.Themes.Services;
using Orchard.Themes.ViewModels;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

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
        private readonly IReportsCoordinator _reportsCoordinator;

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
            IReportsCoordinator reportsCoordinator) {
            Services = services;

            _extensionDisplayEventHandler = extensionDisplayEventHandlers.FirstOrDefault();
            _dataMigrationManager = dataMigraitonManager;
            _siteThemeService = siteThemeService;
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _featureManager = featureManager;
            _previewTheme = previewTheme;
            _themeService = themeService;
            _reportsCoordinator = reportsCoordinator;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            try {
                bool installThemes = _featureManager.GetEnabledFeatures().FirstOrDefault(f => f.Id == "PackagingServices") != null;

                var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();
                ThemeEntry currentTheme = new ThemeEntry(_siteThemeService.GetSiteTheme());
                IEnumerable<ThemeEntry> themes = _extensionManager.AvailableExtensions()
                    .Where(extensionDescriptor => {
                            bool hidden = false;
                            string tags = extensionDescriptor.Tags;
                            if (tags != null) {
                                hidden = tags.Split(',').Any(t => t.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase));
                            }

                            return !hidden &&
                                    DefaultExtensionTypes.IsTheme(extensionDescriptor.ExtensionType) &&
                                    !currentTheme.Descriptor.Id.Equals(extensionDescriptor.Id);
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
            } catch (Exception exception) {
                this.Error(exception, T("Listing themes failed: {0}", exception.Message), Logger, Services.Notifier);

                return View(new ThemesIndexViewModel());
            }
        }

        [HttpPost, FormValueAbsent("submit.Apply"), FormValueAbsent("submit.Cancel")]
        public ActionResult Preview(string themeName, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();

                _themeService.EnableThemeFeatures(themeName);
                _previewTheme.SetPreviewTheme(themeName);

                return this.RedirectLocal(returnUrl, "~/");
            } catch (Exception exception) {
                this.Error(exception, T("Previewing theme failed: {0}", exception.Message), Logger, Services.Notifier);

                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Apply")]
        public ActionResult ApplyPreview(string themeName, string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();
                _previewTheme.SetPreviewTheme(null);
                _siteThemeService.SetSiteTheme(themeName);
            } catch (Exception exception) {
                this.Error(exception, T("Previewing theme failed: {0}", exception.Message), Logger, Services.Notifier);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Preview"), FormValueRequired("submit.Cancel")]
        public ActionResult CancelPreview(string returnUrl) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't preview the current theme")))
                    return new HttpUnauthorizedResult();
                _previewTheme.SetPreviewTheme(null);
            } catch (Exception exception) {
                this.Error(exception, T("Previewing theme failed: {0}", exception.Message), Logger, Services.Notifier);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't enable the theme")))
                    return new HttpUnauthorizedResult();

                _themeService.EnableThemeFeatures(themeName);
            } catch (Exception exception) {
                this.Error(exception, T("Enabling theme failed: {0}", exception.Message), Logger, Services.Notifier);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't disable the current theme")))
                    return new HttpUnauthorizedResult();

                _themeService.DisableThemeFeatures(themeName);
            } catch (Exception exception) {
                this.Error(exception, T("Disabling theme failed: {0}", exception.Message), Logger, Services.Notifier);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Activate(string themeName) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ApplyTheme, T("Couldn't set the current theme")))
                    return new HttpUnauthorizedResult();

                _themeService.EnableThemeFeatures(themeName);
                _siteThemeService.SetSiteTheme(themeName);
            } catch (Exception exception) {
                this.Error(exception, T("Activating theme failed: {0}", exception.Message), Logger, Services.Notifier);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(string themeName) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Couldn't update theme")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(themeName))
                return HttpNotFound();

            try {
                _reportsCoordinator.Register("Data Migration", "Upgrade " + themeName, "Orchard installation");
                _dataMigrationManager.Update(themeName);
                Services.Notifier.Information(T("The theme {0} was updated succesfuly", themeName));
            } catch (Exception exception) {
                this.Error(exception, T("An error occured while updating the theme {0}: {1}", themeName, exception.Message), Logger, Services.Notifier);
            }

            return RedirectToAction("Index");
        }

        class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueRequiredAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return !string.IsNullOrEmpty(value);
            }
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
    }
}