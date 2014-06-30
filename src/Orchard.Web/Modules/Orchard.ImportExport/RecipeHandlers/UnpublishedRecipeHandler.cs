using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.ImportExport.RecipeHandlers {
    [OrchardFeature("Orchard.Deployment")]
    public class UnpublishedRecipeHandler : IRecipeHandler {
        private readonly IOrchardServices _orchardServices;
        public UnpublishedRecipeHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        // <Data />
        // Import Data
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, UnpublishedExportEventHandler.StepName, StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var importContentSession = new ImportContentSession(_orchardServices.ContentManager);

            foreach (var element in recipeContext.RecipeStep.Step.Elements()) {
                if (element.Attribute("Id") == null)
                    continue;

                var identity = element.Attribute("Id").Value;
                if (!string.IsNullOrEmpty(identity)) {
                    var item = importContentSession.Get(identity, VersionOptions.Published, element.Name.LocalName);
                    if (item != null) {
                        _orchardServices.ContentManager.Unpublish(item);
                    }
                }
            }

            recipeContext.Executed = true;
        }
    }
}
