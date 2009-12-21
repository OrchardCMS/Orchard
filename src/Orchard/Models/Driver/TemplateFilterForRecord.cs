using Orchard.Models.Records;
using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class TemplateFilterForRecord<TRecord> : TemplateFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
        private readonly string _prefix;
        private readonly string _templateName;

        public TemplateFilterForRecord(string prefix, string templateName) {
            _prefix = prefix;
            _templateName = templateName;
        }

        protected override void BuildEditorModel(BuildEditorModelContext context, ContentPart<TRecord> part) {
            context.AddEditor(new TemplateViewModel(part.Record, _prefix) { TemplateName = _templateName });
        }

        protected override void UpdateEditorModel(UpdateEditorModelContext context, ContentPart<TRecord> part) {
            context.Updater.TryUpdateModel(part.Record, _prefix, null, null);
            context.AddEditor(new TemplateViewModel(part.Record, _prefix) { TemplateName = _templateName });
        }
    }
}
