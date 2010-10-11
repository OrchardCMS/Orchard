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
            var model = new SearchSettingsViewModel();
            var searchedFields = part.SearchedFields;

            if (_indexManager.HasIndexProvider()) {
                model.Entries = new List<SearchSettingsEntry>();
                foreach (var field in _indexManager.GetSearchIndexProvider().GetFields(SearchIndexName)) {
                    model.Entries.Add(new SearchSettingsEntry { Field = field, Selected = searchedFields.Contains(field) });
                }
            }

            return ContentPartTemplate(model, "Parts/Search.SiteSettings");
        }

        protected override DriverResult Editor(SearchSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new SearchSettingsViewModel();
            
            if(updater.TryUpdateModel(model, Prefix, null, null)) {
                part.SearchedFields = model.Entries.Where(e => e.Selected).Select(e => e.Field).ToArray();
            }

            return Editor(part, shapeHelper);
        }
    }
}