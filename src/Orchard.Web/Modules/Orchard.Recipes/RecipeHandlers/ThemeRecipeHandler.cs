using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ThemeRecipeHandler : IRecipeHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IPackageManager _packageManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IThemeService _themeService;
        private readonly ISiteThemeService _siteThemeService;

        public ThemeRecipeHandler(
            IPackagingSourceManager packagingSourceManager, 
            IPackageManager packageManager,
            IExtensionManager extensionManager,
            IThemeService themeService,
            ISiteThemeService siteThemeService) {

            _packagingSourceManager = packagingSourceManager;
            _packageManager = packageManager;
            _extensionManager = extensionManager;
            _themeService = themeService;
            _siteThemeService = siteThemeService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        // <Theme packageId="theme1" repository="somethemerepo" version="1.1" enable="true" current="true" />
        // install themes from feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Theme", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool enable = false, current = false;
            string packageId = null, version = null, repository = null;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
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
                    Logger.Error("Unrecognized attribute {0} encountered in step Theme. Skipping.", attribute.Name.LocalName);
                }
            }

            if (packageId == null) {
                throw new InvalidOperationException("PackageId is required in a Theme declaration in a recipe file.");
            }

            // download and install theme from the orchard feed or a custom feed if repository is specified.
            bool enforceVersion = version != null;
            bool installed = false;
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
                    _packageManager.Install(packagingEntry.PackageId, packagingEntry.Version, packagingSource.FeedUrl, HostingEnvironment.MapPath("~/"));
                }
                if (current) {
                    _themeService.EnableThemeFeatures(packagingEntry.Title);
                    _siteThemeService.SetSiteTheme(packagingEntry.Title);
                }
                else if (enable) {
                    _themeService.EnableThemeFeatures(packagingEntry.Title);
                }

                installed = true;
            }

            if (!installed) {
                throw new InvalidOperationException(string.Format("Theme {0} was not found in the specified location.", packageId));
            }

            recipeContext.Executed = true;
        }

        private bool ThemeAlreadyInstalled(string packageId) {
            return _extensionManager.AvailableExtensions().Where(t => DefaultExtensionTypes.IsTheme(t.ExtensionType))
                .Any(theme => theme.Id.Equals(
                    packageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme).Length),
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}