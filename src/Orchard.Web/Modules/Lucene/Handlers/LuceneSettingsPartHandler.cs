using Lucene.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Services;
using System.Collections.Generic;

namespace Lucene.Handlers {
    public class LuceneSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public LuceneSettingsPartHandler(IJsonConverter jsonConverter) {
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<LuceneSettingsPart>("Site"));

            OnActivated<LuceneSettingsPart>((context, part) => {
                part.LuceneAnalyzerSelectorMappingsField.Loader(() => {
                    return string.IsNullOrEmpty(part.LuceneAnalyzerSelectorMappingsSerialized)
                        ? new List<LuceneAnalyzerSelectorMapping>()
                        : jsonConverter.Deserialize<List<LuceneAnalyzerSelectorMapping>>(part.LuceneAnalyzerSelectorMappingsSerialized);
                });

                part.LuceneAnalyzerSelectorMappingsField.Setter((value) => {
                    part.LuceneAnalyzerSelectorMappingsSerialized = value == null ? "[]" : jsonConverter.Serialize(value);
                    return value;
                });
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site") return;

            base.GetItemMetadata(context);

            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Lucene Settings")) { Id = "LuceneSettings" });
        }
    }
}