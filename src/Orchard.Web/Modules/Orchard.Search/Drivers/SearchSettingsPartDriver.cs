using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Search.Models;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Drivers {

    public class SearchSettingsPartDriver : ContentPartDriver<SearchSettingsPart> {
        private const string SearchIndexName = "Search"; 
        private readonly IIndexManager _indexManager;

        public SearchSettingsPartDriver(IIndexManager indexManager) {
            _indexManager = indexManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "SearchSettings"; } }

        protected override DriverResult Editor(SearchSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
            
        }

        protected override DriverResult Editor(SearchSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Search_SiteSettings", () => {
                SearchSettingsViewModel model = new SearchSettingsViewModel();
                String[] searchedFields = part.SearchedFields;

                if (updater != null) {
                    // submitting: rebuild model from form data
                    if (updater.TryUpdateModel(model, Prefix, null, null)) {
                        // update part if successful
                        part.SearchedFields = model.Entries.Where(e => e.Selected).Select(e => e.Field).ToArray();
                        part.FilterCulture = model.FilterCulture;
                    }
                }
                else if (_indexManager.HasIndexProvider()) {
                    // viewing editor: build model from part
                    model.FilterCulture = part.FilterCulture;
                    model.Entries = new List<SearchSettingsEntry>();
                    foreach (var field in _indexManager.GetSearchIndexProvider().GetFields(SearchIndexName)) {
                        model.Entries.Add(new SearchSettingsEntry { Field = field, Selected = searchedFields.Contains(field) });
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/Search.SiteSettings", Model: model, Prefix: Prefix);
            }).OnGroup("search");
        }
    }
}