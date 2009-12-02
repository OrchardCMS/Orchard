using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.ViewModels;
using Orchard.Data;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Core.Common.Providers {
    public class BodyAspectProvider : ContentProvider {
        private const string TemplatePrefix = "Body";
        private const string TemplateName = "BodyAspect";
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";

        public BodyAspectProvider(IRepository<BodyRecord> bodyRepository) {
            Filters.Add(new StorageFilter<BodyRecord>(bodyRepository) { AutomaticallyCreateMissingRecord = true });

            OnGetDisplays<BodyAspect>((context, body) => {
                var model = new BodyDisplayViewModel { BodyAspect = body };
                context.AddDisplay(new TemplateViewModel(model, TemplatePrefix) { TemplateName = TemplateName, Position = "3" });
            });

            OnGetEditors<BodyAspect>((context, body) => {
                var model = new BodyEditorViewModel { BodyAspect = body, TextEditorTemplate = DefaultTextEditorTemplate };
                context.AddEditor(new TemplateViewModel(model, TemplatePrefix) { TemplateName = TemplateName, Position = "3" });
            });

            OnUpdateEditors<BodyAspect>((context, body) => {
                var model = new BodyEditorViewModel { BodyAspect = body, TextEditorTemplate = DefaultTextEditorTemplate };
                context.Updater.TryUpdateModel(model, TemplatePrefix, null, null);
                context.AddEditor(new TemplateViewModel(model, TemplatePrefix) { TemplateName = TemplateName, Position = "3" });
            });
        }
    }
}
