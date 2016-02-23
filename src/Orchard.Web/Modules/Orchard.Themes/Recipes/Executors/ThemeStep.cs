using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Themes.Recipes.Executors {
    public class ThemeStep : RecipeExecutionStep {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IPackageManager _packageManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IThemeService _themeService;
        private readonly ISiteThemeService _siteThemeService;

        public ThemeStep(
            IPackagingSourceManager packagingSourceManager, 
            IPackageManager packageManager,
            IExtensionManager extensionManager,
            IThemeService themeService,
            ISiteThemeService siteThemeService,
            RecipeExecutionLogger logger) : base(logger) {

            _packagingSourceManager = packagingSourceManager;
            _packageManager = packageManager;
            _extensionManager = extensionManager;
            _themeService = themeService;
            _siteThemeService = siteThemeService;
        }

        public override string Name { get { return "Theme"; } }

        // <Theme packageId="theme1" repository="somethemerepo" version="1.1" enable="true" current="true" />
        // Install themes from feed.
        public override void Execute(RecipeExecutionContext context) {
            bool enable = false, current = false;
            string packageId = null, version = null, repository = null;

            foreach (var attribute in context.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "enable", StringComparison.OrdinalIgnoreCase)) {
                    enable = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "current", StringComparison.OrdinalIgnoreCase)) {
                    current = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "packageId", StringComparison.OrdinalIgnoreCase)) {
                    packageId = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "version", StringComparison.OrdinalIgnoreCase)) {
                    version = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "repository", StringComparison.OrdinalIgnoreCase)) {
                    repository = attribute.Value;
                }
                else {
                    Logger.Warning("Unrecognized attribute '{0}' encountered; skipping.", attribute.Name.LocalName);
                }
            }

            if (packageId == null) {
                throw new InvalidOperationException("The PackageId attribute is required on a Theme declaration in a recipe file.");
            }

            // Download and install theme from the orchard feed or a custom feed if repository is specified.
            var enforceVersion = version != null;
            var installed = false;
            PackagingEntry packagingEntry = null;

            var packagingSource = _packagingSourceManager.GetSources().FirstOrDefault();
            if (repository != null) {
                packagingSource = new PackagingSource { FeedTitle = repository, FeedUrl = repository };
            }

            if (enforceVersion) {
                packagingEntry = _packagingSourceManager.GetExtensionList(false, packagingSource,
                    packages => packages.Where(package =>
                        package.PackageType.Equals(DefaultExtensionTypes.Theme) &&
                        package.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase) &&
                        package.Version.Equals(version, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
            }
            else {
                packagingEntry = _packagingSourceManager.GetExtensionList(false, packagingSource,
                    packages => packages.Where(package =>
                        package.PackageType.Equals(DefaultExtensionTypes.Theme) &&
                        package.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase) &&
                        package.IsLatestVersion)).FirstOrDefault();
            }

            if (packagingEntry != null) {
                if (!ThemeAlreadyInstalled(packagingEntry.PackageId)) {
                    Logger.Information("Installing theme package '{0}'.", packagingEntry.PackageId);
                    _packageManager.Install(packagingEntry.PackageId, packagingEntry.Version, packagingSource.FeedUrl, HostingEnvironment.MapPath("~/"));
                }
                if (current) {
                    Logger.Information("Enabling theme '{0}'.", packagingEntry.Title);
                    _themeService.EnableThemeFeatures(packagingEntry.Title);
                    Logger.Information("Setting theme '{0}' as the site theme.", packagingEntry.Title);
                    _siteThemeService.SetSiteTheme(packagingEntry.Title);
                }
                else if (enable) {
                    Logger.Information("Enabling theme '{0}'.", packagingEntry.Title);
                    _themeService.EnableThemeFeatures(packagingEntry.Title);
                }

                installed = true;
            }

            if (!installed) {
                throw new InvalidOperationException(String.Format("Theme '{0}' was not found in the specified location.", packageId));
            }
        }

        private bool ThemeAlreadyInstalled(string packageId) {
            return _extensionManager.AvailableExtensions().Where(t => DefaultExtensionTypes.IsTheme(t.ExtensionType))
                .Any(theme => theme.Id.Equals(
                    packageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme).Length),
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}