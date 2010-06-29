using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Indexing.Settings {
    public class IndexingSettings {
        public bool IncludeInIndex { get; set; }
    }

    public class IndexingSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartDefinition.Field definition) {
            var model = definition.Settings.GetModel<IndexingSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new IndexingSettings();
            updateModel.TryUpdateModel(model, "IndexingSettings", null, null);
            builder.WithSetting("IndexingSettings.IncludeInIndex", model.IncludeInIndex ? true.ToString() : null);
            yield return DefinitionTemplate(model);
        }
    }
}
