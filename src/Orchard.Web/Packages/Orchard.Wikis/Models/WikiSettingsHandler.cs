using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Wikis.Models {
    public class WikiSettingsHandler : ContentHandler {
        public WikiSettingsHandler(IRepository<WikiSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<ContentPartForRecord<WikiSettingsRecord>>("site"));
            Filters.Add(new StorageFilterForRecord<WikiSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
        }

        protected override void GetEditors(GetContentEditorsContext context) {
            var part = context.ContentItem.As<ContentPartForRecord<WikiSettingsRecord>>();
            if (part == null)
                return;

            context.Editors.Add(ModelTemplate.For(part.Record, "WikiSettings"));
        }

        protected override void UpdateEditors(UpdateContentContext context) {
            var part = context.ContentItem.As<ContentPartForRecord<WikiSettingsRecord>>();
            if (part == null)
                return;

            context.Updater.TryUpdateModel(part.Record, "WikiSettings", null, null);
            context.Editors.Add(ModelTemplate.For(part.Record, "WikiSettings"));
        }
    }
}