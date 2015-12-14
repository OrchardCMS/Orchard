using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Search.Models;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Drivers {

    public class SearchSettingsPartDriver : ContentPartDriver<SearchSettingsPart> {
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
                var model = new SearchSettingsViewModel();
                var searchFields = part.SearchFields;

                model.DisplayType = part.DisplayType;

                if (updater != null) {
                    if (updater.TryUpdateModel(model, Prefix, null, null)) {
                        part.SearchIndex = model.SelectedIndex;
                        part.SearchFields = model.Entries.ToDictionary(x => x.Index, x => x.Fields.Where(e => e.Selected).Select(e => e.Field).ToArray());
                        part.FilterCulture = model.FilterCulture;
                        part.DisplayType = model.DisplayType;
                    }
                }
                else if (_indexManager.HasIndexProvider()) {
                    model.FilterCulture = part.FilterCulture;
                    model.SelectedIndex = part.SearchIndex;
                    model.Entries = _indexManager.GetSearchIndexProvider().List().Select(x => {
                        var indexSettings = new IndexSettingsEntry {
                            Index = x,
                            Fields = new List<SearchSettingsEntry>()
                        };
                        foreach (var field in _indexManager.GetSearchIndexProvider().GetFields(x)) {
                            indexSettings.Fields.Add(new SearchSettingsEntry {Field = field, Selected = (searchFields.ContainsKey(x) && searchFields[x].Contains(field))});
                        }

                        return indexSettings;
                    }).ToList();
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/Search.SiteSettings", Model: model, Prefix: Prefix);
            }).OnGroup("search");
        }

        protected override void Exporting(SearchSettingsPart part, ExportContentContext context) {
            var searchFields = part.Retrieve<string>("SearchFields");
            if(!String.IsNullOrWhiteSpace(searchFields)) {
                context.Element(part.PartDefinition.Name).Add(new XAttribute("SearchFields", searchFields));
            }            
        }

        protected override void Importing(SearchSettingsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "SearchFields", value => {
                part.Store("SearchFields", value);
            });
        }
    }
}