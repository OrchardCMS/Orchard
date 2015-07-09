using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers {
    public class DataRecipeHandler : IRecipeHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly ITransactionManager _transactionManager;

        public DataRecipeHandler(IOrchardServices orchardServices, ITransactionManager transactionManager) {
            _orchardServices = orchardServices;
            _transactionManager = transactionManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        // <Data />
        // Import Data
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Data", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            Logger.Information("Executing recipe step '{0}'; ExecutionId={1}", recipeContext.RecipeStep.Name, recipeContext.ExecutionId);

            var importContentSession = new ImportContentSession(_orchardServices.ContentManager);

            // Populate local dictionary with elements and their ids
            var elementDictionary = CreateElementDictionary(recipeContext.RecipeStep.Step);

            //Populate import session with all identities to be imported
            foreach (var identity in elementDictionary.Keys) {
                importContentSession.Set(identity, elementDictionary[identity].Name.LocalName);
            }

            //Determine if the import is to be batched in multiple transactions
            var startIndex = 0;
            int batchSize = GetBatchSizeForDataStep(recipeContext.RecipeStep.Step);
            Logger.Debug("Using batch size {0}.", batchSize);

            //Run the import
            try {
                while (startIndex < elementDictionary.Count) {
                    Logger.Debug("Importing batch starting at index {0}.", startIndex);
                    importContentSession.InitializeBatch(startIndex, batchSize);

                    //the session determines which items are included in the current batch
                    //so that dependencies can be managed within the same transaction
                    var nextIdentity = importContentSession.GetNextInBatch();
                    while (nextIdentity != null) {
                        var itemId = "";
                        if (elementDictionary[nextIdentity.ToString()].HasAttributes) {
                            itemId = elementDictionary[nextIdentity.ToString()].FirstAttribute.Value;
                        }
                        Logger.Information("Importing data item '{0}'.", itemId);
                        try {
                            _orchardServices.ContentManager.Import(
                                elementDictionary[nextIdentity.ToString()],
                                importContentSession);
                        }
                        catch (Exception ex) {
                            Logger.Error(ex, "Error while importing data item '{0}'.", itemId);
                            throw;
                        }
                        nextIdentity = importContentSession.GetNextInBatch();
                    }

                    startIndex += batchSize;

                    //Create a new transaction for each batch
                    if (startIndex < elementDictionary.Count) {
                        _transactionManager.RequireNew();
                    }

                    Logger.Debug("Finished importing batch starting at index {0}.", startIndex);
                }
            }
            catch (Exception) {
                //Ensure a failed batch is rolled back
                _transactionManager.Cancel();
                throw;
            }

            recipeContext.Executed = true;
            Logger.Information("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
        }

        private Dictionary<string, XElement> CreateElementDictionary(XElement step) {
            var elementDictionary = new Dictionary<string, XElement>();
            foreach (var element in step.Elements()) {
                if (element.Attribute("Id") == null
                    || string.IsNullOrEmpty(element.Attribute("Id").Value))
                    continue;

                var identity = new ContentIdentity(element.Attribute("Id").Value).ToString();
                elementDictionary[identity] = element;
            }
            return elementDictionary;
        }

        private int GetBatchSizeForDataStep(XElement step) {
            int batchSize;
            if (step.Attribute("BatchSize") == null ||
                !int.TryParse(step.Attribute("BatchSize").Value, out batchSize) ||
                batchSize <= 0) {
                batchSize = int.MaxValue;
            }
            return batchSize;
        }
    }
}
