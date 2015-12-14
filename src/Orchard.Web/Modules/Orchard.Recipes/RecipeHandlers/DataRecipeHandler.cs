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
        private readonly IRecipeJournal _recipeJournal;

        public DataRecipeHandler(IOrchardServices orchardServices, ITransactionManager transactionManager, IRecipeJournal recipeJournal) {
            _orchardServices = orchardServices;
            _transactionManager = transactionManager;
            _recipeJournal = recipeJournal;
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

            var importContentSession = new ImportContentSession(_orchardServices.ContentManager);

            // Populate local dictionary with elements and their ids
            var elementDictionary = CreateElementDictionary(recipeContext.RecipeStep.Step);

            //Populate import session with all identities to be imported
            foreach (var identity in elementDictionary.Keys) {
                Logger.Debug("Importing {0}", identity);
                importContentSession.Set(identity, elementDictionary[identity].Name.LocalName);
            }

            //Determine if the import is to be batched in multiple transactions
            var startIndex = 0;
            int batchSize = GetBatchSizeForDataStep(recipeContext.RecipeStep.Step);

            //Run the import
            try {
                while (startIndex < elementDictionary.Count) {
                    importContentSession.InitializeBatch(startIndex, batchSize);

                    //the session determines which items are included in the current batch
                    //so that dependencies can be managed within the same transaction
                    var nextIdentity = importContentSession.GetNextInBatch();
                    while (nextIdentity != null) {
                        if (!string.IsNullOrEmpty(recipeContext.ExecutionId) && elementDictionary[nextIdentity.ToString()].HasAttributes) {
                            var itemId = elementDictionary[nextIdentity.ToString()].FirstAttribute.Value;
                            _recipeJournal.WriteJournalEntry(recipeContext.ExecutionId, T("Data: Importing {0}.", itemId).Text);
                        }
                        _orchardServices.ContentManager.Import(
                            elementDictionary[nextIdentity.ToString()],
                            importContentSession);
                        nextIdentity = importContentSession.GetNextInBatch();
                    }

                    startIndex += batchSize;

                    //Create a new transaction for each batch
                    if (startIndex < elementDictionary.Count) {
                        _transactionManager.RequireNew();
                    }
                }
            }
            catch (Exception) {
                //Ensure a failed batch is rolled back
                _transactionManager.Cancel();
                throw;
            }

            recipeContext.Executed = true;
        }

        private Dictionary<string, XElement> CreateElementDictionary(XElement step) {
            var elementDictionary = new Dictionary<string, XElement>();
            foreach (var element in step.Elements()) {
                if (element.Attribute("Id") == null
                    || String.IsNullOrWhiteSpace(element.Attribute("Id").Value))
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
