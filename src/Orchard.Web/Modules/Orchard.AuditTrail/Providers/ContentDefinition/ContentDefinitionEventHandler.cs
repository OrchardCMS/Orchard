using System.Collections.Generic;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    public class ContentDefinitionEventHandler : IContentDefinitionEventHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

        public ContentDefinitionEventHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
        }

        public void ContentTypeCreated(dynamic context) {
            RecordContentTypeAuditTrailEvent(ContentTypeAuditTrailEventProvider.Created, context.ContentTypeDefinition);
        }

        public void ContentTypeRemoved(dynamic context) {
            RecordContentTypeAuditTrailEvent(ContentTypeAuditTrailEventProvider.Removed, context.ContentTypeDefinition);
        }

        public void ContentPartCreated(dynamic context) {
            RecordContentPartAuditTrailEvent(ContentPartAuditTrailEventProvider.Created, context.ContentPartDefinition);
        }

        public void ContentPartRemoved(dynamic context) {
            RecordContentPartAuditTrailEvent(ContentPartAuditTrailEventProvider.Removed, context.ContentPartDefinition);
        }

        public void ContentPartAttached(dynamic context) {
            RecordContentTypePartAuditTrailEvent(ContentTypeAuditTrailEventProvider.PartAdded, context.ContentTypeName, context.ContentPartName);
        }

        public void ContentPartDetached(dynamic context) {
            RecordContentTypePartAuditTrailEvent(ContentTypeAuditTrailEventProvider.PartRemoved, context.ContentTypeName, context.ContentPartName);
        }

        public void ContentFieldAttached(dynamic context) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", context.ContentPartName},
                {"ContentFieldName", context.ContentFieldName},
                {"ContentFieldTypeName", context.ContentFieldTypeName},
                {"ContentFieldDisplayName", context.ContentFieldDisplayName}
            };
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(ContentPartAuditTrailEventProvider.FieldAdded, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: context.ContentPartName);
        }

        public void ContentFieldDetached(dynamic context) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", context.ContentPartName},
                {"ContentFieldName", context.ContentFieldName}
            };
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(ContentPartAuditTrailEventProvider.FieldRemoved, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: context.ContentPartName);
        }

        private void RecordContentTypeAuditTrailEvent(string eventName, ContentTypeDefinition contentTypeDefinition) {
            var eventData = new Dictionary<string, object> {
                {"ContentTypeName", contentTypeDefinition.Name},
                {"ContentTypeDisplayName", contentTypeDefinition.DisplayName},
            };
            _auditTrailManager.CreateRecord<ContentTypeAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contenttype", eventFilterData: contentTypeDefinition.Name);
        }

        private void RecordContentPartAuditTrailEvent(string eventName, ContentPartDefinition contentPartDefinition) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", contentPartDefinition.Name}
            };
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: contentPartDefinition.Name);
        }

        private void RecordContentTypePartAuditTrailEvent(string eventName, string contentTypeName, string contentPartName) {
            var eventData = new Dictionary<string, object> {
                {"ContentTypeName", contentTypeName},
                {"ContentPartName", contentPartName}
            };
            _auditTrailManager.CreateRecord<ContentTypeAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contenttype", eventFilterData: contentTypeName);
        }
    }
}