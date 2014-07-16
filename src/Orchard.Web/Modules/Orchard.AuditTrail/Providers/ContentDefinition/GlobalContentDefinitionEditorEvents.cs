using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Services;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class GlobalContentDefinitionEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentDefinitionService _contentDefinitionService;
        private string _oldContentTypeDisplayName;
        private SettingsDictionary _oldContentTypeSettings;
        private readonly ISettingsFormatter _settingsFormatter;

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
            var contentType = _contentDefinitionService.GetType(definition.Name);
            _oldContentTypeDisplayName = contentType.DisplayName;
            _oldContentTypeSettings = new SettingsDictionary(contentType.Settings);
        }

        public override void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) {
            var contentTypeDefinition = builder.Build();
            var newDisplayName = contentTypeDefinition.DisplayName;
            var newSettings = contentTypeDefinition.Settings;

            if (newDisplayName != _oldContentTypeDisplayName) {
                var eventData = new Dictionary<string, object> {
                    {"ContentTypeName", builder.Name},
                    {"OldDisplayName", _oldContentTypeDisplayName},
                    {"NewDisplayName", newDisplayName}
                };
                RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.TypeDisplayNameUpdated, eventData, contentTypeDefinition.Name);
            }

            if (!AreEqual(newSettings, _oldContentTypeSettings)) {
                var eventData = new Dictionary<string, object> {
                    {"ContentTypeName", builder.Name},
                    {"OldSettings", ToXml(_oldContentTypeSettings)},
                    {"NewSettings", ToXml(newSettings)}
                };
                RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.TypeSettingsUpdated, eventData, contentTypeDefinition.Name);
            }
        }

        public override void TypePartEditorUpdating(ContentTypePartDefinitionBuilder builder) {
            // TODO: record current values
        }

        public override void TypePartEditorUpdated(ContentTypePartDefinitionBuilder builder) {
            // TODO: compare old values with new values.
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", builder.Name},
                {"ContentTypeName", builder.TypeName}
            };
            RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.PartSettingsUpdated, eventData, builder.TypeName);
        }

        //public override void PartEditorUpdated(ContentPartDefinitionBuilder builder) {
        //    var eventData = new Dictionary<string, object> {
        //        {"ContentPartName", builder.Name}
        //    };
        //    RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.PartSettingsUpdated, eventData, builder.Name);
        //}

        //public override void PartFieldEditorUpdated(ContentPartFieldDefinitionBuilder builder) {
        //    var eventData = new Dictionary<string, object> {
        //        {"ContentFieldName", builder.Name},
        //        {"ContentFieldType", builder.FieldType},
        //        {"ContentPartName", builder.PartName}
        //    };
        //    RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.FieldSettingsUpdated, eventData, builder.PartName);
        //}

        private void RecordContentTypeAuditTrail(string eventName, IDictionary<string, object> eventData, string contentTypeName) {
            _auditTrailManager.CreateRecord<ContentTypeAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contenttype", eventFilterData: contentTypeName);
        }

        private void RecordContentPartAuditTrail(string eventName, IDictionary<string, object> eventData, string contentPartName) {
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: contentPartName);
        }

        private string ToXml(SettingsDictionary settings) {
            return _settingsFormatter.Map(settings).ToString(SaveOptions.DisableFormatting);
        }

        private bool AreEqual(SettingsDictionary a, SettingsDictionary b) {
            var xml1 = ToXml(a);
            var xml2 = ToXml(b);

            return String.Equals(xml1, xml2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
