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
        ILogger Logger { get; set; }

        // <Theme name="theme1" repository="somethemerepo" version="1.1" enable="true" current="true" />
        // install themes from feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Theme", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool enable = false, current = false;
            string name = null, version = null, repository = null;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "enable", StringComparison.OrdinalIgnoreCase)) {
                    enable = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "current", StringComparison.OrdinalIgnoreCase)) {
                    current = Boolean.Parse(attribute.Value);
                }
                else if (String.Equals(attribute.Name.LocalName, "name", StringComparison.OrdinalIgnoreCase)) {
                    name = attribute.Value;
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

            if (name == null) {
                throw new InvalidOperationException("Name is required in a Theme declaration in a recipe file.");
            }

            // download and install theme from the orchard feed or a custom feed if repository is specified.
            bool enforceVersion = version != null;
            bool installed = false;

            PackagingSource packagingSource = _packagingSourceManager.GetSources().FirstOrDefault();
            if (repository != null) {
                enforceVersion = false;
                packagingSource = new PackagingSource { FeedTitle = repository, FeedUrl = repository };
            }

            if (_extensionManager.AvailableExtensions().Where(extension =>
                DefaultExtensionTypes.IsTheme(extension.ExtensionType) &&
                extension.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Any()) {
                    throw new InvalidOperationException(string.Format("Theme {0} already exists.", name));
            }

            PackagingEntry packagingEntry = _packagingSourceManager.GetExtensionList(packagingSource,
                packages => packages.Where(package =>
                    package.PackageType.Equals(DefaultExtensionTypes.Theme) &&
                    package.Title.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                    (!enforceVersion || package.Version.Equals(version, StringComparison.OrdinalIgnoreCase))))
                .FirstOrDefault();

            if (packagingEntry != null) {
                _packageManager.Install(packagingEntry.PackageId, packagingEntry.Version, packagingSource.FeedUrl, HostingEnvironment.MapPath("~/"));

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
                throw new InvalidOperationException(string.Format("Theme {0} was not found in the specified location.", name));
            }

            recipeContext.Executed = true;
        }
    }
}