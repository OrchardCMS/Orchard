using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Search.Models;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Drivers {

    [OrchardFeature("Orchard.Search.Content")]
    public class AdminSearchSettingsPartDriver : ContentPartDriver<AdminSearchSettingsPart> {
        private readonly IIndexManager _indexManager;

        public AdminSearchSettingsPartDriver(IIndexManager indexManager) {
            _indexManager = indexManager;
        }

        protected override DriverResult Editor(AdminSearchSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }

        protected override DriverResult Editor(AdminSearchSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_AdminSearch_SiteSettings", () => {
                var model = new AdminSearchSettingsViewModel {
                    AvailableIndexes = _indexManager.GetSearchIndexProvider().List().ToList(),
                    SelectedIndex = part.SearchIndex
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(model, Prefix, null, new[] { "AvailableIndexes" })) {
                        part.SearchIndex = model.SelectedIndex;
                    }
                }
                
                return shapeHelper.EditorTemplate(TemplateName: "Parts/AdminSearch.SiteSettings", Model: model, Prefix: Prefix);
            }).OnGroup("search");
        }
    }
}