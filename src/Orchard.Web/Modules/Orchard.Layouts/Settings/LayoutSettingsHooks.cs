using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Layouts.Framework.Serialization;
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

            if (String.IsNullOrWhiteSpace(model.Flavor)) {
                var partModel = definition.PartDefinition.Settings.GetModel<LayoutPartSettings>();
                model.Flavor = partModel.FlavorDefault;
            }

            if (String.IsNullOrWhiteSpace(model.DefaultLayoutState)) {
                var defaultState = _serializer.Serialize(_layoutManager.CreateDefaultLayout());
                model.DefaultLayoutState = defaultState;
            }

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "LayoutPart")
                yield break;

            var model = new LayoutTypePartSettings();
            updateModel.TryUpdateModel(model, "LayoutTypePartSettings", null, null);
            builder.WithSetting("LayoutTypePartSettings.Flavor", !String.IsNullOrWhiteSpace(model.Flavor) ? model.Flavor : null);
            builder.WithSetting("LayoutTypePartSettings.IsTemplate", model.IsTemplate.ToString());
            builder.WithSetting("LayoutTypePartSettings.DefaultLayoutState", model.DefaultLayoutState);
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
            if (definition.Name != "LayoutPart")
                yield break;

            var model = definition.Settings.GetModel<LayoutPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "LayoutPart")
                yield break;

            var model = new LayoutPartSettings();
            updateModel.TryUpdateModel(model, "LayoutPartSettings", null, null);
            builder.WithSetting("LayoutPartSettings.FlavorDefault", !String.IsNullOrWhiteSpace(model.FlavorDefault) ? model.FlavorDefault : null);
            yield return DefinitionTemplate(model);
        }
    }
}