using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Settings {
    public class LayoutSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly ILayoutSerializer _serializer;
        private readonly ILayoutManager _layoutManager;

        public LayoutSettingsHooks(ILayoutSerializer serializer, ILayoutManager layoutManager) {
            _serializer = serializer;
            _layoutManager = layoutManager;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "LayoutPart")
                yield break;

            var model = definition.Settings.GetModel<LayoutTypePartSettings>();
            
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "LayoutPart")
                yield break;

            var model = new LayoutTypePartSettings();
            updateModel.TryUpdateModel(model, "LayoutTypePartSettings", null, null);
            builder.WithSetting("LayoutTypePartSettings.IsTemplate", model.IsTemplate.ToString());
            builder.WithSetting("LayoutTypePartSettings.DefaultLayoutData", model.DefaultLayoutData);
            yield return DefinitionTemplate(model);
        }
    }
}