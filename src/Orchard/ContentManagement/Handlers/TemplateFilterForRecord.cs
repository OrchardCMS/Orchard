using System;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    [Obsolete]
    public class TemplateFilterForRecord<TRecord> : TemplateFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
        private readonly string _prefix;
        private string _location;
        private string _position;

        public TemplateFilterForRecord(string prefix) {
            _prefix = prefix;
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

        //protected override void BuildEditorShape(BuildEditorModelContext context, ContentPart<TRecord> part) {
        //    context.ContentItem.Zones[_location].Add(part.Record, _position);
        //}

        //protected override void UpdateEditorShape(UpdateEditorModelContext context, ContentPart<TRecord> part) {
        //    context.Updater.TryUpdateModel(part.Record, _prefix, null, null);
        //    BuildEditorShape(context, part);
        //}
    }
}
