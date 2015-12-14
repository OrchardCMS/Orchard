using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;
using NuGet;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Modules.Services;
using Orchard.Mvc.Extensions;
using Orchard.Packaging.Extensions;
using Orchard.Packaging.Services;
using Orchard.Packaging.ViewModels;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;
using PackageBuilder = Orchard.Packaging.Services.PackageBuilder;

namespace Orchard.Packaging.Controllers {
    [Themed, Admin]
    public class PackagingServicesController : Controller {

        private readonly ShellSettings _shellSettings;
        private readonly IPackageManager _packageManager;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IAppDataFolderRoot _appDataFolderRoot;
        private readonly IModuleService _moduleService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeManager _recipeManager;

        public PackagingServicesController(
            ShellSettings shellSettings,
            IPackageManager packageManager,
            IPackagingSourceManager packagingSourceManager,
            IAppDataFolderRoot appDataFolderRoot,
            IOrchardServices services,
            IModuleService moduleService,
            IHostEnvironment hostEnvironment)
            : this(shellSettings, packageManager, packagingSourceManager, appDataFolderRoot, services, moduleService, hostEnvironment, null, null) {
        }

        public PackagingServicesController(
            ShellSettings shellSettings,
            IPackageManager packageManager,
            IPackagingSourceManager packagingSourceManager,
            IAppDataFolderRoot appDataFolderRoot,
            IOrchardServices services,
            IModuleService moduleService,
            IHostEnvironment hostEnvironment,
            IRecipeHarvester recipeHarvester,
            IRecipeManager recipeManager) {

            _shellSettings = shellSettings;
            _packageManager = packageManager;
            _appDataFolderRoot = appDataFolderRoot;
            _moduleService = moduleService;
            _hostEnvironment = hostEnvironment;
            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
            _packagingSourceManager = packagingSourceManager;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = Logging.NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }
        public Logging.ILogger Logger { get; set; }

        public ActionResult AddTheme(string returnUrl) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add themes")))
                return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost, ActionName("UninstallTheme")]
        public ActionResult UninstallThemePost(string themeId, string returnUrl, string retryUrl) {
            if (String.IsNullOrEmpty(themeId)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to remove themes")))
                return new HttpUnauthorizedResult();

            return UninstallPackage(PackageBuilder.BuildPackageId(themeId, DefaultExtensionTypes.Theme), returnUrl, retryUrl);
        }

        [HttpPost, ActionName("UninstallModule")]
        public ActionResult UninstallModulePost(string moduleId, string returnUrl, string retryUrl) {
            if (String.IsNullOrEmpty(moduleId)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to remove modules")))
                return new HttpUnauthorizedResult();

            return UninstallPackage(PackageBuilder.BuildPackageId(moduleId, DefaultExtensionTypes.Module), returnUrl, retryUrl);
        }

        public ActionResult AddModule(string returnUrl) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add modules")))
                return new HttpUnauthorizedResult();

            return View();
        }

        public ActionResult InstallGallery(string packageId, string version, int sourceId, string redirectUrl) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            var source = _packagingSourceManager.GetSources().FirstOrDefault(s => s.Id == sourceId);
            if (source == null) {
                return HttpNotFound();
            }

           try {
                PackageInfo packageInfo = _packageManager.Install(packageId, version, source.FeedUrl, MapAppRoot());

                if (DefaultExtensionTypes.IsTheme(packageInfo.ExtensionType)) {
                    Services.Notifier.Information(T("The theme has been successfully installed. It can be enabled in the \"Themes\" page accessible from the menu."));
                } 
                else if (DefaultExtensionTypes.IsModule(packageInfo.ExtensionType)) {
                    Services.Notifier.Information(T("The module has been successfully installed."));

                    IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource(source.FeedUrl, "Default"));
                    IPackage package = packageRepository.FindPackage(packageId);
                    ExtensionDescriptor extensionDescriptor = package.GetExtensionDescriptor(packageInfo.ExtensionType);

                    return InstallPackageDetails(extensionDescriptor, redirectUrl);
                }
            }
           catch (OrchardException e) {
               Services.Notifier.Error(T("Package installation failed: {0}", e.Message));
               return View("InstallPackageFailed");
           }
           catch (Exception) {
               Services.Notifier.Error(T("Package installation failed."));
               return View("InstallPackageFailed");
           }   

           return Redirect(redirectUrl);
        }

        public ActionResult InstallLocally(string redirectUrl) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to install packages")))
                return new HttpUnauthorizedResult();

            var httpPostedFileBase = Request.Files.Get(0);
            if (httpPostedFileBase == null 
                || Request.Files.Count == 0 
                || string.IsNullOrWhiteSpace(httpPostedFileBase.FileName)) {

                throw new OrchardException(T("Select a file to upload."));
            }
            try {
                string fullFileName = Path.Combine(_appDataFolderRoot.RootFolder, Path.GetFileName(httpPostedFileBase.FileName)).Replace(Path.DirectorySeparatorChar, '/');
                httpPostedFileBase.SaveAs(fullFileName);
                var package = new ZipPackage(fullFileName);
                PackageInfo packageInfo = _packageManager.Install(package, _appDataFolderRoot.RootFolder, MapAppRoot());
                ExtensionDescriptor extensionDescriptor = package.GetExtensionDescriptor(packageInfo.ExtensionType);
                System.IO.File.Delete(fullFileName);

                if (DefaultExtensionTypes.IsTheme(extensionDescriptor.ExtensionType)) {
                    Services.Notifier.Information(T("The theme has been successfully installed. It can be enabled in the \"Themes\" page accessible from the menu."));
                }
                else if (DefaultExtensionTypes.IsModule(extensionDescriptor.ExtensionType)) {
                    Services.Notifier.Information(T("The module has been successfully installed."));

                    return InstallPackageDetails(extensionDescriptor, redirectUrl);
                }
            }
            catch (OrchardException e) {
                Services.Notifier.Error(T("Package uploading and installation failed: {0}", e.Message));
                return View("InstallPackageFailed");
            }
            catch (Exception) {
                Services.Notifier.Error(T("Package uploading and installation failed."));
                return View("InstallPackageFailed");
            }

            return Redirect(redirectUrl);
        }

        private ActionResult InstallPackageDetails(ExtensionDescriptor extensionDescriptor, string redirectUrl) {
            if (DefaultExtensionTypes.IsModule(extensionDescriptor.ExtensionType)) {
                List<PackagingInstallFeatureViewModel> features = extensionDescriptor.Features
                    .Where(featureDescriptor => !DefaultExtensionTypes.IsTheme(featureDescriptor.Extension.ExtensionType))
                    .Select(featureDescriptor => new PackagingInstallFeatureViewModel {
                        Enable = true, // by default all features are enabled
                        FeatureDescriptor = featureDescriptor
                    }).ToList();

                List<PackagingInstallRecipeViewModel> recipes = null;
                if (_recipeHarvester != null) {
                    recipes = _recipeHarvester.HarvestRecipes(extensionDescriptor.Id)
                        .Select(recipe => new PackagingInstallRecipeViewModel {
                            Execute = false, // by default no recipes are executed
                            Recipe = recipe
                        }).ToList();
                }

                if (features.Count > 0) {
                    return View("InstallModuleDetails", new PackagingInstallViewModel {
                        Features = features,
                        Recipes = recipes,
                        ExtensionDescriptor = extensionDescriptor
                    });
                }
            }

            return Redirect(redirectUrl);
        }

        [HttpPost, ActionName("InstallPackageDetails")]
        public ActionResult InstallPackageDetailsPOST(PackagingInstallViewModel packagingInstallViewModel, string redirectUrl) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            try {
                if (_recipeHarvester != null && _recipeManager != null) {
                    IEnumerable<Recipe> recipes = _recipeHarvester.HarvestRecipes(packagingInstallViewModel.ExtensionDescriptor.Id)
                        .Where(loadedRecipe => packagingInstallViewModel.Recipes.FirstOrDefault(recipeViewModel => recipeViewModel.Execute && recipeViewModel.Recipe.Name.Equals(loadedRecipe.Name)) != null);

                    foreach (Recipe recipe in recipes) {
                        try {
                            _recipeManager.Execute(recipe);
                        }
                        catch {
                            Services.Notifier.Error(T("Recipes contains {0} unsupported module installation steps.", recipe.Name));
                        }
                    }
                }

                // Enable selected features
                if (packagingInstallViewModel.Features.Count > 0) {
                    IEnumerable<string> featureIds = packagingInstallViewModel.Features
                        .Where(feature => feature.Enable)
                        .Select(feature => feature.FeatureDescriptor.Id);

                    // Enable the features and its dependencies using recipes, so that they are run after the module's recipes

                    var recipe = new Recipe {
                        RecipeSteps = featureIds.Select(
                            x => new RecipeStep {
                                Name = "Feature",
                                Step = new XElement("Feature", new XAttribute("enable", x))
                            })
                    };

                    _recipeManager.Execute(recipe);
                }
            } catch (Exception exception) {
                Services.Notifier.Error(T("Post installation steps failed with error: {0}", exception.Message));
            }

            return Redirect(redirectUrl);
        }

        private ActionResult UninstallPackage(string id, string returnUrl, string retryUrl) {
            try {
                _packageManager.Uninstall(id, MapAppRoot());
            }
            catch (Exception exception) {
                Services.Notifier.Error(T("Uninstall failed: {0}", exception.Message));
                return Redirect(!String.IsNullOrEmpty(retryUrl) ? retryUrl : returnUrl);
            }

            Services.Notifier.Information(T("Uninstalled package \"{0}\"", id));
            return this.RedirectLocal(returnUrl, "~/");
        }

        private string MapAppRoot() {
            return _hostEnvironment.MapPath("~/");
        }
    }
}