using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Events;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Services;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class ContentDefinitionEventHandler : IContentDefinitionEventHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;
        private XElement _previousContentTypeXml;
        private XElement _previousContentPartXml;
        private readonly IDiffGramAnalyzer _analyzer;

        public ContentDefinitionEventHandler(
            IAuditTrailManager auditTrailManager, 
            IWorkContextAccessor wca, 
            IContentDefinitionWriter contentDefinitionWriter, 
            IDiffGramAnalyzer analyzer) {

            _auditTrailManager = auditTrailManager;
            _wca = wca;
            _contentDefinitionWriter = contentDefinitionWriter;
            _analyzer = analyzer;
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context) {
            RecordContentTypeAuditTrailEvent(ContentTypeAuditTrailEventProvider.Created, context.ContentTypeDefinition);
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) {
            RecordContentTypeAuditTrailEvent(ContentTypeAuditTrailEventProvider.Removed, context.ContentTypeDefinition);
        }

        public void ContentTypeImporting(ContentTypeImportingContext context) {
            _previousContentTypeXml = context.ContentTypeDefinition != null ? _contentDefinitionWriter.Export(context.ContentTypeDefinition) : default(XElement);
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            var newContentTypeXml = _contentDefinitionWriter.Export(context.ContentTypeDefinition);
            RecordContentTypeAuditTrailEvent(ContentTypeAuditTrailEventProvider.Imported, context.ContentTypeDefinition, _previousContentTypeXml, newContentTypeXml);
        }

        public void ContentPartCreated(ContentPartCreatedContext context) {
            RecordContentPartAuditTrailEvent(ContentPartAuditTrailEventProvider.Created, context.ContentPartDefinition);
        }

        public void ContentPartRemoved(ContentPartRemovedContext context) {
            RecordContentPartAuditTrailEvent(ContentPartAuditTrailEventProvider.Removed, context.ContentPartDefinition);
        }

        public void ContentPartAttached(ContentPartAttachedContext context) {
            RecordContentTypePartAuditTrailEvent(ContentTypeAuditTrailEventProvider.PartAdded, context.ContentTypeName, context.ContentPartName);
        }

        public void ContentPartDetached(ContentPartDetachedContext context) {
            RecordContentTypePartAuditTrailEvent(ContentTypeAuditTrailEventProvider.PartRemoved, context.ContentTypeName, context.ContentPartName);
        }

        public void ContentPartImporting(ContentPartImportingContext context) {
            _previousContentPartXml = context.ContentPartDefinition != null ? _contentDefinitionWriter.Export(context.ContentPartDefinition) : default(XElement);
        }

        public void ContentPartImported(ContentPartImportedContext context) {
            var newContentPartXml = _contentDefinitionWriter.Export(context.ContentPartDefinition);
            RecordContentPartAuditTrailEvent(ContentPartAuditTrailEventProvider.Imported, context.ContentPartDefinition, _previousContentPartXml, newContentPartXml);
        }

        public void ContentFieldAttached(ContentFieldAttachedContext context) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", context.ContentPartName},
                {"ContentFieldName", context.ContentFieldName},
                {"ContentFieldTypeName", context.ContentFieldTypeName},
                {"ContentFieldDisplayName", context.ContentFieldDisplayName}
            };
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(ContentPartAuditTrailEventProvider.FieldAdded, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: context.ContentPartName);
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context) {
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", context.ContentPartName},
                {"ContentFieldName", context.ContentFieldName}
            };
            _auditTrailManager.CreateRecord<ContentPartAuditTrailEventProvider>(ContentPartAuditTrailEventProvider.FieldRemoved, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contentpart", eventFilterData: context.ContentPartName);
        }

        private void RecordContentTypeAuditTrailEvent(string eventName, ContentTypeDefinition contentTypeDefinition, XElement previousContentTypeXml = null, XElement newContentTypeXml = null) {
            var record = true;
            var eventData = new Dictionary<string, object> {
                {"ContentTypeName", contentTypeDefinition.Name},
                {"ContentTypeDisplayName", contentTypeDefinition.DisplayName},
            };

            if (previousContentTypeXml != null || newContentTypeXml != null) {
                var previousXml = previousContentTypeXml != null ? previousContentTypeXml.ToString(SaveOptions.DisableFormatting) : default(string);
                var newXml = newContentTypeXml != null ? newContentTypeXml.ToString(SaveOptions.DisableFormatting) : default(string);

                if (previousXml != null) {
                    if (previousXml != newXml) {
                        var diffGram = _analyzer.GenerateDiffGram(previousContentTypeXml, newContentTypeXml);
                        eventData["PreviousDefinition"] = previousXml;
                        eventData["NewDefinition"] = newXml;
                        eventData["DiffGram"] = diffGram.ToString(SaveOptions.DisableFormatting);
                    }
                    else {
                        record = false;
                    }
                }
                else {
                    eventData["NewDefinition"] = newXml;
                }
            }

            if (record)
                _auditTrailManager.CreateRecord<ContentTypeAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties: null, eventData: eventData, eventFilterKey: "contenttype", eventFilterData: contentTypeDefinition.Name);
        }

        private void RecordContentPartAuditTrailEvent(string eventName, ContentPartDefinition contentPartDefinition, XElement previousContentPartXml = null, XElement newContentPartXml = null) {
            var record = true;
            var eventData = new Dictionary<string, object> {
                {"ContentPartName", contentPartDefinition.Name}
            };

            if (previousContentPartXml != null || newContentPartXml != null) {
                var previousXml = previousContentPartXml != null ?previousContentPartXml.ToString(SaveOptions.DisableFormatting) : default(string);
                var newXml = newContentPartXml != null ? newContentPartXml.ToString(SaveOptions.DisableFormatting) : default(string);

                if (previousXml != null) {
                    if (previousXml != newXml) {
                        var diffGram = _analyzer.GenerateDiffGram(previousContentPartXml, newContentPartXml);
                        eventData["PreviousDefinition"] = previousXml;
                        eventData["NewDefinition"] = newXml;
                        eventData["DiffGram"] = diffGram.ToString(SaveOptions.DisableFormatting);
                    }
                    else {
                        record = false;
                    }
                }
                else {
                    eventData["NewDefinition"] = newXml;
                }
            }

            if (record)
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