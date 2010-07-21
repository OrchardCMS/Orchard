using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Common.Settings {
    public class LocationSettingsEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypeDefinition.Part definition) {
            yield return TypePartEditorForLocation(definition, "DisplayLocation");
            yield return TypePartEditorForLocation(definition, "EditorLocation");
        }

        private TemplateViewModel TypePartEditorForLocation(ContentTypeDefinition.Part definition, string locationSettings) {
            // Look for the setting in the most specific settings first (part definition in type)
            // then in the global part definition.
            var settings =
                definition.Settings.TryGetModel<LocationSettings>(locationSettings) ??
                definition.PartDefinition.Settings.GetModel<LocationSettings>(locationSettings);

            return DefinitionTemplate(settings, locationSettings, locationSettings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypeDefinitionBuilder.PartConfigurer builder, IUpdateModel updateModel) {
            yield return TypePartEditorUpdateForLocation(builder, updateModel, "DisplayLocation");
            yield return TypePartEditorUpdateForLocation(builder, updateModel, "EditorLocation");
        }

        private TemplateViewModel TypePartEditorUpdateForLocation(ContentTypeDefinitionBuilder.PartConfigurer builder, IUpdateModel updateModel, string locationSettings) {
            var locationsettings = new LocationSettings();
            updateModel.TryUpdateModel(locationsettings, locationSettings, null, null);
            builder.WithLocation("EditorLocation", locationsettings.Zone, locationsettings.Position);
            return DefinitionTemplate(locationsettings, locationSettings, locationSettings);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartDefinition.Field definition) {
            yield return PartFieldEditorForLocation(definition, "DisplayLocation");
            yield return PartFieldEditorForLocation(definition, "EditorLocation");
        }

        private TemplateViewModel PartFieldEditorForLocation(ContentPartDefinition.Field definition, string locationSettings) {
            var settings = definition.Settings.GetModel<LocationSettings>(locationSettings);
            return DefinitionTemplate(settings, locationSettings, locationSettings);
        }


        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartDefinitionBuilder.FieldConfigurer builder, IUpdateModel updateModel) {
            yield return PartFieldEditorUpdateForLocation(builder, updateModel, "DisplayLocation");
            yield return PartFieldEditorUpdateForLocation(builder, updateModel, "EditorLocation");
        }

        private TemplateViewModel PartFieldEditorUpdateForLocation(ContentPartDefinitionBuilder.FieldConfigurer builder, IUpdateModel updateModel, string locationSettings) {
            var model = new LocationSettings();
            updateModel.TryUpdateModel(model, locationSettings, null, null);
            builder.WithLocation(locationSettings, model.Zone, model.Position);
            return DefinitionTemplate(model);
        }
    }
}