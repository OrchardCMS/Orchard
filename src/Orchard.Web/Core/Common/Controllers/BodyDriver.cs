using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Core.Common.Controllers {
    public class BodyDriver : PartDriver<BodyAspect> {        
        private const string TemplateName = "Parts/Common.Body";
        private const string DefaultTextEditorTemplate = "TinyMceTextEditor";
        protected override string Prefix {
            get {return "Body";}
        }

        protected override DriverResult Display(BodyAspect part, string displayType) {
            var model = new BodyDisplayViewModel { BodyAspect = part };
            return PartTemplate(model, TemplateName, Prefix).Location("body");
        }

        protected override DriverResult Editor(BodyAspect part) {
            var model = new BodyEditorViewModel { BodyAspect = part, TextEditorTemplate = DefaultTextEditorTemplate };
            return PartTemplate(model, TemplateName, Prefix).Location("body");
        }

        protected override DriverResult Editor(BodyAspect part, Orchard.ContentManagement.IUpdateModel updater) {
            var model = new BodyEditorViewModel { BodyAspect = part, TextEditorTemplate = DefaultTextEditorTemplate };
            updater.TryUpdateModel(model, Prefix, null, null);
            return PartTemplate(model, TemplateName, Prefix).Location("body");
        }
    }
}
