using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Indexing;
using Orchard.Search.Models;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Drivers {
    public class SearchFormPartDriver : ContentPartDriver<SearchFormPart> {
        private readonly IIndexManager _indexManager;
        public SearchFormPartDriver(IIndexManager indexManager) {
            _indexManager = indexManager;
        }

        protected override DriverResult Display(SearchFormPart part, string displayType, dynamic shapeHelper) {
            var model = new SearchViewModel();
            return ContentShape("Parts_Search_SearchForm", () => {
                var shape = shapeHelper.Parts_Search_SearchForm();
                shape.AvailableIndexes = _indexManager.GetSearchIndexProvider().List().ToList();
                shape.ContentPart = part;
                shape.ViewModel = model;
                return shape;
            });
        }

        protected override DriverResult Editor(SearchFormPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(SearchFormPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Search_SearchForm_Edit", () => {
                var viewModel = new SearchFormViewModel {
                    OverrideIndex = part.OverrideIndex,
                    AvailableIndexes = _indexManager.GetSearchIndexProvider().List().ToList(),
                    SelectedIndex = part.SelectedIndex
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, new[] {"AvailableIndexes"})) {
                        part.OverrideIndex = viewModel.OverrideIndex;
                        part.SelectedIndex = viewModel.SelectedIndex;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/Search.SearchForm", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override void Exporting(SearchFormPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("OverrideIndex", part.OverrideIndex);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SelectedIndex", part.SelectedIndex);
        }

        protected override void Importing(SearchFormPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "OverrideIndex", x => part.OverrideIndex = XmlHelper.Parse<bool>(x));
            context.ImportAttribute(part.PartDefinition.Name, "SelectedIndex", x => part.SelectedIndex = x);
        }
    }
}