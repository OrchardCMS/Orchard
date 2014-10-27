using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentTypes.Services;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class GlobalContentDefinitionEditorEvents : ContentDefinitionEditorEventsBase {
        private const string _contentPartSettingsDescriptionName = "ContentPartSettings.Description";
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly ISettingsFormatter _settingsFormatter;
        private string _oldContentTypeDisplayName;
        private SettingsDictionary _oldContentTypeSettings;
        private SettingsDictionary _oldContentTypePartSettings;
        private SettingsDictionary _oldContentPartFieldSettings;
        private SettingsDictionary _oldPartSettings;

        public GlobalContentDefinitionEditorEvents(
            IAuditTrailManager auditTrailManager, 
            IWorkContextAccessor wca, 
            IContentDefinitionService contentDefinitionService, 
            ISettingsFormatter settingsFormatter) {

            _auditTrailManager = auditTrailManager;
            _wca = wca;
            _contentDefinitionService = contentDefinitionService;
            _settingsFormatter = settingsFormatter;
        }

        public override void TypeEditorUpdating(ContentTypeDefinitionBuilder definition) {
            var contentType = _contentDefinitionService.GetType(definition.Current.Name);
            _oldContentTypeDisplayName = contentType.DisplayName;
            _oldContentTypeSettings = new SettingsDictionary(contentType.Settings);
        }

        public override void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) {
            var contentTypeDefinition = builder.Build();
            var newDisplayName = contentTypeDefinition.DisplayName;
            var newSettings = contentTypeDefinition.Settings;

            if (newDisplayName != _oldContentTypeDisplayName) {
                var eventData = new Dictionary<string, object> {
                    {"ContentTypeName", builder.Current.Name},
                    {"OldDisplayName", _oldContentTypeDisplayName},
                    {"NewDisplayName", newDisplayName}
                };
                RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.TypeDisplayNameUpdated, eventData, contentTypeDefinition.Name);
            }

            if (!AreEqual(newSettings, _oldContentTypeSettings)) {
                var eventData = new Dictionary<string, object> {
                    {"ContentTypeName", builder.Current.Name},
                    {"OldSettings", ToXml(_oldContentTypeSettings)},
                    {"NewSettings", ToXml(newSettings)}
                };
                RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.TypeSettingsUpdated, eventData, contentTypeDefinition.Name);
            }
        }

        public override void TypePartEditorUpdating(ContentTypePartDefinitionBuilder builder) {
            var contentTypeDefinition = _contentDefinitionService.GetType(builder.TypeName);
            var contentPart = contentTypeDefinition.Parts.Single(x => x.PartDefinition.Name == builder.Name);
            _oldContentTypePartSettings = contentPart.Settings;
        }

        public override void TypePartEditorUpdated(ContentTypePartDefinitionBuilder builder) {
            var contentTypePartDefinition = builder.Build();
            var newSettings = contentTypePartDefinition.Settings;

            if (!AreEqual(newSettings, _oldContentTypePartSettings)) {
                var eventData = new Dictionary<string, object> {
                    {"ContentPartName", builder.Name},
                    {"ContentTypeName", builder.TypeName},
                    {"OldSettings", ToXml(_oldContentTypePartSettings)},
                    {"NewSettings", ToXml(newSettings)}
                };
                RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.PartSettingsUpdated, eventData, builder.TypeName);
            }
        }

        public override void PartFieldEditorUpdating(ContentPartFieldDefinitionBuilder builder) {
            var contentPart = _contentDefinitionService.GetPart(builder.PartName);
            var contentField = contentPart.Fields.Single(x => x.Name == builder.Name);
            _oldContentPartFieldSettings = contentField.Settings;
        }

        public override void PartFieldEditorUpdated(ContentPartFieldDefinitionBuilder builder) {
            var contentPartFieldDefinition = builder.Build();
            var newSettings = contentPartFieldDefinition.Settings;

            if (!AreEqual(newSettings, _oldContentPartFieldSettings)) {
                var eventData = new Dictionary<string, object> {
                    {"ContentFieldName", builder.Name},
                    {"ContentPartName", builder.PartName},
                    {"OldSettings", ToXml(_oldContentPartFieldSettings)},
                    {"NewSettings", ToXml(newSettings)}
                };
                RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.FieldSettingsUpdated, eventData, builder.PartName);
            }
        }

        public override void PartEditorUpdating(ContentPartDefinitionBuilder builder) {
            var contentPart = _contentDefinitionService.GetPart(builder.Name);
            _oldPartSettings = contentPart.Settings;
        }

        public override void PartEditorUpdated(ContentPartDefinitionBuilder builder) {
            var contentPartDefinition = builder.Build();
            var newPartSettings = contentPartDefinition.Settings;

            if (newPartSettings.ContainsKey(_contentPartSettingsDescriptionName)) {
                var oldDescription = _oldPartSettings.Get(_contentPartSettingsDescriptionName);
                var newDescription = newPartSettings.Get(_contentPartSettingsDescriptionName);
                if (oldDescription != newDescription) {
                    var eventData = new Dictionary<string, object> {
                        {"ContentPartName", builder.Name},
                        {"OldDescription", oldDescription},
                        {"NewDescription", newDescription}
                    };
                    RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.DescriptionChanged, eventData, builder.Name);
                }
            }

            // Description change should not be re-recorded as general settings change.
            var remainingOldPartSettings = new SettingsDictionary(_oldPartSettings.Where(item => item.Key != _contentPartSettingsDescriptionName).ToDictionary(item => item.Key, item => item.Value));
            var remainingNewPartSettings = new SettingsDictionary(newPartSettings.Where(item => item.Key != _contentPartSettingsDescriptionName).ToDictionary(item => item.Key, item => item.Value));

            if (!AreEqual(remainingNewPartSettings, remainingOldPartSettings)) {
                var eventData = new Dictionary<string, object> {
                    {"ContentPartName", builder.Name},
                    {"OldSettings", ToXml(remainingOldPartSettings)},
                    {"NewSettings", ToXml(remainingNewPartSettings)}
                };
                RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.PartSettingsUpdated, eventData, builder.Name);
            }
        }

        private void RecordContentTypeAuditTrail(string eventName, IDictionary<string, object> eventData, string contentTypeName) {
            _auditTrailManager.CreateRecord<ContentTypeAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contenttype", eventFilterData: contentTypeName);
        }

        private void RecordContentPartAuditTrail(string eventName, IDictionary<string, object> eventData, string contentPartName) {
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: contentPartName);
        }

        private string ToXml(SettingsDictionary settings) {
            return settings.ToXml(_settingsFormatter);
        }

        private bool AreEqual(SettingsDictionary a, SettingsDictionary b) {
            return a.IsEqualTo(b, _settingsFormatter);
        }
    }
}
