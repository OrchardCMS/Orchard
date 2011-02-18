using System;
using System.Collections;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class ModuleRecipeHandler : IRecipeHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;

        public ModuleRecipeHandler(IPackagingSourceManager packagingSourceManager) {
            _packagingSourceManager = packagingSourceManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        // <Module src="http://" replace="false" />
        // <Module name="module1" [repository="somerepo"] version="1.1" replace="true" />
        // install modules from url or feed.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Module", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            bool replace;
            string source = null, name = null, version = null, repository = null;

            foreach (var attribute in recipeContext.RecipeStep.Step.Attributes()) {
                if (String.Equals(attribute.Name.LocalName, "src", StringComparison.OrdinalIgnoreCase)) {
                    source = attribute.Value;
                }
                else if (String.Equals(attribute.Name.LocalName, "replace", StringComparison.OrdinalIgnoreCase)) {
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

            if (source != null) {
            }
            else {
                if (name == null) {
                    throw new InvalidOperationException("Either name or source is required in a Module declaration in a recipe file.");
                }
                // download and install module from the orchard feed or a custom feed if repository is specified.
                bool enforceVersion = version != null;
                bool installed = false;
                PackagingSource packagingSource = null;
                if (repository != null) {
                    enforceVersion = false;
                    packagingSource = new PackagingSource {FeedTitle = repository, FeedUrl = repository};
                }
                foreach (var packagingEntry in _packagingSourceManager.GetExtensionList(packagingSource)) {
                    if (String.Equals(packagingEntry.Title, name, StringComparison.OrdinalIgnoreCase)) {
                        if (enforceVersion && !String.Equals(packagingEntry.Version, version, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }
                        // install.
                        installed = true;
                        break;
                    }
                }

                if (!installed) {
                    throw new InvalidOperationException(string.Format("Module {0} was not found in the specified location.", name));
                }
                
            }

            recipeContext.Executed = true;
        }
    }
}