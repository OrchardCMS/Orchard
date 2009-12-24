using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.ViewModels;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Common.Providers {
    public class BodyAspectHandler : ContentHandler {
        private const string TemplatePrefix = "Body";
        private const string TemplateName = "Parts/Common.Body";
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";

        public BodyAspectHandler(IRepository<BodyRecord> bodyRepository) {
            Filters.Add(new StorageFilter<BodyRecord>(bodyRepository) { AutomaticallyCreateMissingRecord = true });

            OnGetDisplayViewModel<BodyAspect>((context, body) => {
                var model = new BodyDisplayViewModel { BodyAspect = body };
                context.AddDisplay(new TemplateViewModel(model, TemplatePrefix) { TemplateName = TemplateName, ZoneName = "body" });
            });

            OnGetEditorViewModel<BodyAspect>((context, body) => {
                var model = new BodyEditorViewModel { BodyAspect = body, TextEditorTemplate = DefaultTextEditorTemplate };
                context.AddEditor(new TemplateViewModel(model, TemplatePrefix) { TemplateName = TemplateName, ZoneName = "body" });
            });

            OnUpdateEditorViewModel<BodyAspect>((context, body) => {
                var model = new BodyEditorViewModel { BodyAspect = body, TextEditorTemplate = DefaultTextEditorTemplate };
                context.Updater.TryUpdateModel(model, TemplatePrefix, null, null);
                context.AddEditor(new TemplateViewModel(model, TemplatePrefix) { TemplateName = TemplateName, ZoneName = "body" });
            });
        }
    }
}
