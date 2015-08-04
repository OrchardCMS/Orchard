using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
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
        private readonly ITransactionManager _transactionManager;

        public RecipesStep(
            IRecipeHarvester recipeHarvester, 
            IRecipeStepQueue recipeStepQueue, 
            IRepository<RecipeStepResultRecord> recipeStepResultRecordRepository, 
            ITransactionManager transactionManager,
            RecipeExecutionLogger logger) : base(logger) {

            _recipeHarvester = recipeHarvester;
            _recipeStepQueue = recipeStepQueue;
            _recipeStepResultRecordRepository = recipeStepResultRecordRepository;
            _transactionManager = transactionManager;
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
            var session = _transactionManager.GetSession();

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

                    EnqueueRecipe(session, context.ExecutionId, recipes[recipeName]);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while executing recipe '{0}' in extension '{1}'.", recipeName, extensionId);
                    throw;
                }
            }
        }

        private void EnqueueRecipe(ISession session, string executionId, Recipe recipe) {
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
            return _recipeHarvester.HarvestRecipes(extensionId).ToDictionary(x => x.Name);
        }
    }
}
