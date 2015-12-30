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

namespace Orchard.Recipes.Providers.Executors {
    public class ModuleStep : RecipeExecutionStep {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IPackageManager _packageManager;
        private readonly IExtensionManager _extensionManager;

        public ModuleStep(
            IPackagingSourceManager packagingSourceManager, 
            IPackageManager packageManager, 
            IExtensionManager extensionManager,
            RecipeExecutionLogger logger) : base(logger) {

            _packagingSourceManager = packagingSourceManager;
            _packageManager = packageManager;
            _extensionManager = extensionManager;
        }

        public override string Name { get { return "Module"; } }

        // <Module packageId="module1" [repository="somerepo"] version="1.1" />
        // Install modules from feed.
        public override void Execute(RecipeExecutionContext context) {
            string packageId = null, version = null, repository = null;
            foreach (var attribute in context.RecipeStep.Step.Attributes()) {
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
                    throw new InvalidOperationException(String.Format("Unrecognized attribute {0} encountered in step Module.", attribute.Name.LocalName));
                }
            }

            if (packageId == null) {
                throw new InvalidOperationException("PackageId is required in a module declaration in a recipe file.");
            }

            // download and install module from the orchard feed or a custom feed if repository is specified.
            var enforceVersion = version != null;
            var installed = false;
            PackagingEntry packagingEntry = null;

            var packagingSource = _packagingSourceManager.GetSources().FirstOrDefault();
            if (repository != null) {
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
                    Logger.Information("Installing module {0}.", packagingEntry.Title);
                    _packageManager.Install(packagingEntry.PackageId, packagingEntry.Version, packagingSource.FeedUrl, HostingEnvironment.MapPath("~/")); 
                }
                installed = true;
            }

            if (!installed) {
                throw new InvalidOperationException(String.Format("Module {0} was not found in the specified location.", packageId));
            }
        }

        private bool ModuleAlreadyInstalled(string packageId) {
            return _extensionManager.AvailableExtensions().Where(m => DefaultExtensionTypes.IsModule(m.ExtensionType))
                .Any(module => module.Id.Equals(
                    packageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module).Length), 
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}