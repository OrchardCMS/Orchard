using System.Collections.Generic;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    public class GlobalContentDefinitionEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

        public GlobalContentDefinitionEditorEvents(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var eventData = new Dictionary<string, object> {
                {"ContentTypeName", builder.Name}
            };
            RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.TypeSettingsUpdated, eventData, builder.Name);
            yield break;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", builder.Name},
                {"ContentTypeName", builder.TypeName}
            };
            RecordContentTypeAuditTrail(ContentTypeAuditTrailEventProvider.PartSettingsUpdated, eventData, builder.TypeName);
            yield break;
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", builder.Name}
            };
            RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.PartSettingsUpdated, eventData, builder.Name);
            yield break;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var eventData = new Dictionary<string, object> {
                {"ContentFieldName", builder.Name},
                {"ContentFieldType", builder.FieldType},
                {"ContentPartName", builder.PartName}
            };
            RecordContentPartAuditTrail(ContentPartAuditTrailEventProvider.FieldSettingsUpdated, eventData, builder.PartName);
            yield break;
        }

        private void RecordContentTypeAuditTrail(string eventName, IDictionary<string, object> eventData, string contentTypeName) {
            _auditTrailManager.CreateRecord<ContentTypeAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contenttype", eventFilterData: contentTypeName);
        }

        private void RecordContentPartAuditTrail(string eventName, IDictionary<string, object> eventData, string contentPartName) {
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: contentPartName);
        }
    }
}
