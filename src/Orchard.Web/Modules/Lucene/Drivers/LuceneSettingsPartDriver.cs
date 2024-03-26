using Lucene.Models;
using Lucene.Services;
using Lucene.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Lucene.Drivers {
    public class LuceneSettingsPartDriver : ContentPartDriver<LuceneSettingsPart> {
        private readonly IIndexManager _indexManager;
        private readonly IEnumerable<ILuceneAnalyzerSelector> _analyzerSelectors;
        private readonly INotifier _notifier;

        public Localizer T { get; set; }

        public LuceneSettingsPartDriver(IIndexManager indexManager, IEnumerable<ILuceneAnalyzerSelector> analyzerSelectors, INotifier notifier) {
            _indexManager = indexManager;
            _analyzerSelectors = analyzerSelectors;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(LuceneSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_LuceneSettings_Edit", () => {
                MaintainMappings(part);
                return shapeHelper.EditorTemplate(
                         TemplateName: "Parts.LuceneSettings",
                         Model: new LuceneSettingsPartEditViewModel {
                             LuceneAnalyzerSelectorMappings = part.LuceneAnalyzerSelectorMappings.ToArray(),
                             LuceneAnalyzerSelectors = _analyzerSelectors.Select(analyzerSelector =>
                                 new SelectListItem { Text = T(analyzerSelector.Name).Text, Value = analyzerSelector.Name })
                         },
                         Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(LuceneSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new LuceneSettingsPartEditViewModel();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                _notifier.Warning(T("Don't forget to rebuild your index in case you have changed its analyzer."));
                part.LuceneAnalyzerSelectorMappings = viewModel.LuceneAnalyzerSelectorMappings;
                MaintainMappings(part);
            }

            return Editor(part, shapeHelper);
        }

        private void MaintainMappings(LuceneSettingsPart part) {
            var analyzerProviderNames = _analyzerSelectors.Select(analyzerProvider => analyzerProvider.Name);
            var indexNames = _indexManager.GetSearchIndexProvider().List();
            var maintainedMappings = part.LuceneAnalyzerSelectorMappings.ToList();
            // Removing mappings which contain a removed/invalid index or analyzer provider.
            maintainedMappings.RemoveAll(mapping => !indexNames.Contains(mapping.IndexName) || !analyzerProviderNames.Contains(mapping.AnalyzerName));
            // Adding new mappings for the new indexes.
            foreach (var indexName in indexNames) {
                if (!maintainedMappings.Any(mapping => mapping.IndexName == indexName)) {
                    maintainedMappings.Add(new LuceneAnalyzerSelectorMapping { IndexName = indexName, AnalyzerName = "Default" });
                }
            }

            part.LuceneAnalyzerSelectorMappings = maintainedMappings;
        }
    }
}