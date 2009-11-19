using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Models.Records;
using Orchard.UI.Models;

namespace Orchard.Models.Driver {
    public class TemplateFilterForRecord<TRecord> : TemplateFilterBase<ContentPartForRecord<TRecord>> where TRecord : ContentPartRecord, new() {
        private readonly string _prefix;

        public TemplateFilterForRecord(string prefix) {
            _prefix = prefix;
        }

        protected override void GetEditors(GetContentEditorsContext context, ContentPartForRecord<TRecord> part) {
            context.Editors.Add(ModelTemplate.For(part.Record, _prefix));
        }

        protected override void UpdateEditors(UpdateContentContext context, ContentPartForRecord<TRecord> part) {
            context.Updater.TryUpdateModel(part.Record, _prefix, null, null);
            context.Editors.Add(ModelTemplate.For(part.Record, _prefix));
        }
    }
}
