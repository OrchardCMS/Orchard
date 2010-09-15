using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class TemplateFilterForRecord<TRecord> : TemplateFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
        private readonly string _prefix;
        private string _location;
        private string _position;
        private readonly string _templateName;

        public TemplateFilterForRecord(string prefix, string templateName) {
            _prefix = prefix;
            _templateName = templateName;
            _location = "primary";
            _position = "5";
        }

        public TemplateFilterForRecord<TRecord> Location(string location) {
            _location = location;
            return this;
        }

        public TemplateFilterForRecord<TRecord> Position(string position) {
            _position = position;
            return this;
        }

        protected override void BuildEditorShape(BuildEditorModelContext context, ContentPart<TRecord> part) {
            var templateShape = context.Shape.EditorTemplate(TemplateName: _templateName, Model: part.Record, Prefix: _prefix);
            context.Model.Zones[_location].Add(templateShape, _position);
        }

        protected override void UpdateEditorShape(UpdateEditorModelContext context, ContentPart<TRecord> part) {
            context.Updater.TryUpdateModel(part.Record, _prefix, null, null);
            BuildEditorShape(context, part);
        }
    }
}
