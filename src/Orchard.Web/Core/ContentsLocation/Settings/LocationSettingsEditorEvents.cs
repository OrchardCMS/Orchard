using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.ContentsLocation.ViewModels;
using Orchard.Localization;

namespace Orchard.Core.ContentsLocation.Settings {
    public class LocationSettingsEditorEvents : ContentDefinitionEditorEventsBase {

        public LocationSettingsEditorEvents() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private IEnumerable<LocationDefinition> GetPredefinedLocations() {
            yield return new LocationDefinition { Name = "Default", DisplayName = T("Default location (i.e. fallback if no specific override)") };
            yield return new LocationDefinition { Name = "Detail", DisplayName = T("Location in a \"Detail\" screen") };
            yield return new LocationDefinition { Name = "Editor", DisplayName = T("Location in a \"Editor\" screen") };
            yield return new LocationDefinition { Name = "Summary", DisplayName = T("Location in a \"Summary\" screen (Front-end)") };
            yield return new LocationDefinition { Name = "SummaryAdmin", DisplayName = T("Location in a \"Summary\" screen (Admin)") };
        }

        private LocationSettings MergeSettings(LocationSettings partSettings, LocationSettings partDefinitionSettings) {
            return partSettings;
            //var result = new LocationSettings(partSettings);
            //foreach (var entry in partDefinitionSettings) {
            //    if (!partSettings.ContainsKey(entry.Key))
            //        partSettings[entry.Key] = entry.Value;
            //}
            //return result;
        }

        #region Standalone part definition
        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
            var settings = definition.Settings.GetModel<LocationSettings>();

            foreach (var location in GetPredefinedLocations()) {
                var viewModel = new LocationSettingsViewModel {
                    Definition = location,
                    Location = settings.Get(location.Name),
                    DefaultLocation = new ContentLocation()
                };
                yield return DefinitionTemplate(viewModel, templateName: "LocationSettings", prefix: location.Name);
            }
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            var settings = new LocationSettings();
            foreach (var location in GetPredefinedLocations()) {
                var viewModel = new LocationSettingsViewModel();
                updateModel.TryUpdateModel(viewModel, location.Name, null, null);
                settings[location.Name] = viewModel.Location;
                yield return DefinitionTemplate(viewModel, templateName: "LocationSettings", prefix: location.Name);
            }
            builder.WithLocation(settings);
        }
        #endregion

        #region Part in the context of a content type
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            // Look for the setting in the most specific settings first (part definition in type)
            // then in the global part definition.
            var partSettings = definition.Settings.GetModel<LocationSettings>();
            var partDefinitionSettings = definition.PartDefinition.Settings.GetModel<LocationSettings>();
            var settings = MergeSettings(partSettings, partDefinitionSettings);

            foreach (var location in GetPredefinedLocations()) {
                var viewModel = new LocationSettingsViewModel {
                    Definition = location,
                    Location = settings.Get(location.Name),
                    DefaultLocation = partDefinitionSettings.Get(location.Name)
                };
                yield return DefinitionTemplate(viewModel, templateName: "LocationSettings", prefix: location.Name);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            var settings = new LocationSettings();
            foreach (var location in GetPredefinedLocations()) {
                var viewModel = new LocationSettingsViewModel();
                updateModel.TryUpdateModel(viewModel, location.Name, null, null);
                settings[location.Name] = viewModel.Location;
                yield return DefinitionTemplate(viewModel, templateName: "LocationSettings", prefix: location.Name);
            }
            builder.WithLocation(settings);
        }
        #endregion

        #region Field within a content part
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            var settings = definition.Settings.GetModel<LocationSettings>();
            foreach (var location in GetPredefinedLocations()) {
                var viewModel = new LocationSettingsViewModel {
                    Definition = location,
                    Location = settings.Get(location.Name),
                    DefaultLocation = new ContentLocation { Zone = "body", Position = "" }
                };
                yield return DefinitionTemplate(viewModel, templateName: "LocationSettings", prefix: location.Name);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var settings = new LocationSettings();
            foreach (var location in GetPredefinedLocations()) {
                var viewModel = new LocationSettingsViewModel();
                updateModel.TryUpdateModel(viewModel, location.Name, null, null);
                settings[location.Name] = viewModel.Location;
                yield return DefinitionTemplate(viewModel, templateName: "LocationSettings", prefix: location.Name);
            }
            builder.WithLocation(settings);
        }
        #endregion
    }
}