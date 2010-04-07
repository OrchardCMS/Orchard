using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class TemplateFilterForRecord<TRecord> : TemplateFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
        private readonly string _prefix;
        private readonly string _templateName;
        private string _location;

        public TemplateFilterForRecord(string prefix, string templateName) {
            _prefix = prefix;
            _templateName = templateName;
            _location = "primary";
        }

        public TemplateFilterForRecord<TRecord> Location(string location) {
            _location = location;
            return this;
        }

        protected override void BuildEditorModel(BuildEditorModelContext context, ContentPart<TRecord> part) {
            context.ViewModel.Zones.AddEditorPart(_location, part.Record, _templateName, _prefix);
        }

        protected override void UpdateEditorModel(UpdateEditorModelContext context, ContentPart<TRecord> part) {
            context.Updater.TryUpdateModel(part.Record, _prefix, null, null);
            context.ViewModel.Zones.AddEditorPart(_location, part.Record, _templateName, _prefix);
        }
    }
}
