using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.Providers.Executors {
    public class RecipesStep : RecipeExecutionStep {
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRepository<RecipeStepResultRecord> _recipeStepResultRecordRepository;

        public RecipesStep(
            IRecipeHarvester recipeHarvester,
            IRecipeStepQueue recipeStepQueue,
            IRepository<RecipeStepResultRecord> recipeStepResultRecordRepository,
            RecipeExecutionLogger logger) : base(logger) {

            _recipeHarvester = recipeHarvester;
            _recipeStepQueue = recipeStepQueue;
            _recipeStepResultRecordRepository = recipeStepResultRecordRepository;
        }

        public override string Name { get { return "Recipes"; } }

        /*  
         <Recipes>
          <Recipe ExtensionId="Orchard.Setup" Name="Core" />
         </Recipes>
        */
        public override void Execute(RecipeExecutionContext context) {
            var recipeElements = context.RecipeStep.Step.Elements();
            var recipesDictionary = new Dictionary<string, IDictionary<string, Recipe>>();

            foreach (var recipeElement in recipeElements) {
                var extensionId = recipeElement.Attr("ExtensionId");
                var recipeName = recipeElement.Attr("Name");

                Logger.Information("Executing recipe '{0}' in extension '{1}'.", recipeName, extensionId);

                try {
                    var recipes = recipesDictionary.ContainsKey(extensionId) ? recipesDictionary[extensionId] : default(IDictionary<string, Recipe>);
                    if (recipes == null)
                        recipes = recipesDictionary[extensionId] = HarvestRecipes(extensionId);

                    if (!recipes.ContainsKey(recipeName))
                        throw new Exception(String.Format("No recipe named '{0}' was found in extension '{1}'.", recipeName, extensionId));

                    EnqueueRecipe(context.ExecutionId, recipes[recipeName]);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while executing recipe '{0}' in extension '{1}'.", recipeName, extensionId);
                    throw;
                }
            }
        }

        private void EnqueueRecipe(string executionId, Recipe recipe) {
            foreach (var recipeStep in recipe.RecipeSteps) {
                _recipeStepQueue.Enqueue(executionId, recipeStep);
                _recipeStepResultRecordRepository.Create(new RecipeStepResultRecord {
                    ExecutionId = executionId,
                    RecipeName = recipe.Name,
                    StepId = recipeStep.Id,
                    StepName = recipeStep.Name
                });
            }
        }

        private IDictionary<string, Recipe> HarvestRecipes(string extensionId) {
            try {
                return _recipeHarvester.HarvestRecipes(extensionId).ToDictionary(x => x.Name);
            }
            catch (ArgumentException ex) {
                throw new OrchardFatalException(T("A recipe with the same name has been detected for extension \"{0}\". Please make sure recipes are uniquely named.", extensionId), ex);
            }
        }
    }
}
