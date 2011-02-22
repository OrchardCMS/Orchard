using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Modules.Services;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ModuleRecipeHandler : IRecipeHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IPackageManager _packageManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IModuleService _moduleService;
        private readonly IDataMigrationManager _dataMigrationManager;

        public ModuleRecipeHandler(
            IPackagingSourceManager packagingSourceManager, 
            IPackageManager packageManager, 
            IExtensionManager extensionManager,
            IModuleService moduleService,
            IDataMigrationManager dataMigrationManager) {
            _packagingSourceManager = packagingSourceManager;
            _packageManager = packageManager;
            _extensionManager = extensionManager;
            _moduleService = moduleService;
            _dataMigrationManager = dataMigrationManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // <Module name="module1" [repository="somerepo"] version="1.1" replace="false" />
        // install modules from feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Module", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool replace;
            string name = null, version = null, repository = null;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "replace", StringComparison.OrdinalIgnoreCase)) {
                    replace = Boolean.Parse(attribute.Value);
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
                    throw new InvalidOperationException(string.Format("Unrecognized attribute {0} encountered in step Module.", attribute.Name.LocalName));
                }
            }

            if (name == null) {
                throw new InvalidOperationException("Name is required in a Module declaration in a recipe file.");
            }
            // download and install module from the orchard feed or a custom feed if repository is specified.
            bool enforceVersion = version != null;
            bool installed = false;
            PackagingSource packagingSource = _packagingSourceManager.GetSources().FirstOrDefault();
            if (repository != null) {
                enforceVersion = false;
                packagingSource = new PackagingSource {FeedTitle = repository, FeedUrl = repository};
            }
            foreach (var packagingEntry in _packagingSourceManager.GetExtensionList(packagingSource)) {
                if (String.Equals(packagingEntry.Title, name, StringComparison.OrdinalIgnoreCase)) {
                    if (enforceVersion && !String.Equals(packagingEntry.Version, version, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    // use for replace.
                    bool moduleExists = false;
                    foreach (var extension in _extensionManager.AvailableExtensions()
                        .Where(extension => 
                            DefaultExtensionTypes.IsModule(extension.ExtensionType) && 
                            String.Equals(packagingEntry.Title, extension.Name, StringComparison.OrdinalIgnoreCase))) {
                        moduleExists = true;
                    }
                    _packageManager.Install(packagingEntry.PackageId, packagingEntry.Version, packagingSource.FeedUrl, HostingEnvironment.MapPath("~/"));
                    _moduleService.EnableFeatures(new[] { packagingEntry.Title }, true);
                    _dataMigrationManager.Update(packagingEntry.Title);
                    installed = true;
                    break;
                }
            }

            if (!installed) {
                throw new InvalidOperationException(string.Format("Module {0} was not found in the specified location.", name));
            }
                

            recipeContext.Executed = true;
        }
    }
}