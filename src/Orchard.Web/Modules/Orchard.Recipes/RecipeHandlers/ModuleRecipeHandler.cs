using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ModuleRecipeHandler : IRecipeHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IPackageManager _packageManager;
        private readonly IExtensionManager _extensionManager;

        public ModuleRecipeHandler(
            IPackagingSourceManager packagingSourceManager, 
            IPackageManager packageManager, 
            IExtensionManager extensionManager) {
            _packagingSourceManager = packagingSourceManager;
            _packageManager = packageManager;
            _extensionManager = extensionManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        // <Module packageId="module1" [repository="somerepo"] version="1.1" />
        // install modules from feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Module", StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            string packageId = null, version = null, repository = null;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "packageId", StringComparison.OrdinalIgnoreCase)) {
                    packageId = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "version", StringComparison.OrdinalIgnoreCase)) {
                    version = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "repository", StringComparison.OrdinalIgnoreCase)) {
                    repository = attribute.Value;
                }
                else {
                    throw new InvalidOperationException(string.Format("Unrecognized attribute {0} encountered in step Module.", attribute.Name.LocalName));
                }
            }

            if (packageId == null) {
                throw new InvalidOperationException("PackageId is required in a Module declaration in a recipe file.");
            }

            // download and install module from the orchard feed or a custom feed if repository is specified.
            bool enforceVersion = version != null;
            bool installed = false;
            PackagingEntry packagingEntry = null;

            var packagingSource = _packagingSourceManager.GetSources().FirstOrDefault();
            if (repository != null) {
                enforceVersion = false;
                packagingSource = new PackagingSource {FeedTitle = repository, FeedUrl = repository};
            }

            if (enforceVersion) {
                packagingEntry = _packagingSourceManager.GetExtensionList(false, packagingSource, 
                    packages => packages.Where(package => 
                        package.PackageType.Equals(DefaultExtensionTypes.Module) && 
                        package.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase) && 
                        package.Version.Equals(version, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
            }
            else {
                packagingEntry = _packagingSourceManager.GetExtensionList(false, packagingSource, 
                    packages => packages.Where(package => 
                        package.PackageType.Equals(DefaultExtensionTypes.Module) && 
                        package.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase) && 
                        package.IsLatestVersion)).FirstOrDefault();
            }

            if (packagingEntry != null) {
                if (!ModuleAlreadyInstalled(packagingEntry.PackageId)) {
                    _packageManager.Install(packagingEntry.PackageId, packagingEntry.Version, packagingSource.FeedUrl, HostingEnvironment.MapPath("~/")); 
                }
                installed = true;
            }

            if (!installed) {
                throw new InvalidOperationException(string.Format("Module {0} was not found in the specified location.", packageId));
            }

            recipeContext.Executed = true;
        }

        private bool ModuleAlreadyInstalled(string packageId) {
            return _extensionManager.AvailableExtensions().Where(m => DefaultExtensionTypes.IsModule(m.ExtensionType))
                .Any(module => module.Id.Equals(
                    packageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module).Length), 
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}