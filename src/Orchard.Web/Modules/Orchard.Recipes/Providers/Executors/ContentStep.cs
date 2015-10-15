using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Recipes.ViewModels;

namespace Orchard.Recipes.Providers.Executors {
    public class ContentStep : RecipeExecutionStep {
        private readonly IOrchardServices _orchardServices;
        private readonly ITransactionManager _transactionManager;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;

        public ContentStep(
            IOrchardServices orchardServices,
            ITransactionManager transactionManager,
            Lazy<IEnumerable<IContentHandler>> handlers,
            RecipeExecutionLogger logger) : base(logger) {

            _orchardServices = orchardServices;
            _transactionManager = transactionManager;
            _handlers = handlers;
            BatchSize = 64;
        }

        public override string Name {
            get { return "Content"; }
        }

        public override IEnumerable<string> Names {
            get { return new[] { Name, "Data" }; }
        }

        public override LocalizedString DisplayName {
            get { return T("Content"); }
        }

        public override LocalizedString Description {
            get { return T("Provides additional configuration for the Content recipe step."); }
        }

        public int? BatchSize { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new ContentExecutionStepViewModel {
                BatchSize = BatchSize
            };

            if (updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    BatchSize = viewModel.BatchSize;
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "ExecutionSteps/Content", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(RecipeExecutionStepConfigurationContext context) {
            BatchSize = context.ConfigurationElement.Attr<int?>("BatchSize");
        }

        public override void UpdateStep(UpdateRecipeExecutionStepContext context) {
            SetBatchSizeForDataStep(context.Step, BatchSize);
        }

        // <Data />
        // Import Data.
        public override void Execute(RecipeExecutionContext context) {
            // Run the import.
            BatchedInvoke(context, "Import", (itemId, nextIdentityValue, element, importContentSession, elementDictionary) => {
                _orchardServices.ContentManager.Import(element, importContentSession);
            });
            
            // Invoke ImportCompleted.
            BatchedInvoke(context, "ImportCompleted", (itemId, nextIdentityValue, element, importContentSession, elementDictionary) => {
                _orchardServices.ContentManager.CompleteImport(element, importContentSession);
            });
        }

        private void BatchedInvoke(RecipeExecutionContext context, string batchLabel, Action<string, string, XElement, ImportContentSession, IDictionary<string, XElement>> contentItemAction) {
            var importContentSession = new ImportContentSession(_orchardServices.ContentManager);

            // Populate local dictionary with elements and their ids.
            var elementDictionary = CreateElementDictionary(context.RecipeStep.Step);

            // Populate import session with all identities to be imported.
            foreach (var identity in elementDictionary.Keys) {
                importContentSession.Set(identity, elementDictionary[identity].Name.LocalName);
            }

            // Determine if the import is to be batched in multiple transactions.
            var batchSize = GetBatchSizeForDataStep(context.RecipeStep.Step);
            var startIndex = 0;
            var itemIndex = 0;

            Logger.Debug("Using batch size {0} for '{1}'.", batchSize, batchLabel);

            try {
                while (startIndex < elementDictionary.Count) {
                    Logger.Debug("Batch '{0}' execution starting at index {1}.", batchLabel, startIndex);
                    importContentSession.InitializeBatch(startIndex, batchSize);

                    // The session determines which items are included in the current batch
                    // so that dependencies can be managed within the same transaction.
                    var nextIdentity = importContentSession.GetNextInBatch();
                    while (nextIdentity != null) {
                        var itemId = "";
                        var nextIdentityValue = nextIdentity.ToString();
                        if (elementDictionary[nextIdentityValue].HasAttributes) {
                            itemId = elementDictionary[nextIdentityValue].FirstAttribute.Value;
                        }
                        Logger.Information("Handling content item '{0}' (item {1}/{2} of '{3}').", itemId, itemIndex + 1, elementDictionary.Count, batchLabel);
                        try {
                            contentItemAction(itemId, nextIdentityValue, elementDictionary[nextIdentityValue], importContentSession, elementDictionary);
                        }
                        catch (Exception ex) {
                            Logger.Error(ex, "Error while handling content item '{0}' (item {1}/{2} of '{3}').", itemId, itemIndex + 1, elementDictionary.Count, batchLabel);
                            throw;
                        }
                        itemIndex++;
                        nextIdentity = importContentSession.GetNextInBatch();
                    }

                    startIndex += batchSize;

                    // Create a new transaction for each batch.
                    if (startIndex < elementDictionary.Count) {
                        _transactionManager.RequireNew();
                    }

                    Logger.Debug("Finished batch '{0}' starting at index {1}.", batchLabel, startIndex);
                }
            }
            catch (Exception) {
                // Ensure a failed batch is rolled back.
                _transactionManager.Cancel();
                throw;
            }
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

        private void SetBatchSizeForDataStep(XElement step, int? batchSize) {
            step.SetAttributeValue("BatchSize", batchSize);
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
