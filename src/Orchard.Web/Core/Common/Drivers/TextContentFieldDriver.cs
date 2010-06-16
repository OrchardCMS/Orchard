using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class TextContentFieldDriver : ContentFieldDriver<TextContentField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Common.TextContentField";

        public TextContentFieldDriver(IOrchardServices services) {
            Services = services;
        }

        protected override string Prefix {
            get { return "TextContentField"; }
        }

        protected override DriverResult Display(TextContentField field, string displayType) {
            var model = new TextContentFieldDisplayViewModel { Text = field };

            return ContentFieldTemplate(model, TemplateName, Prefix);
            
        }

        protected override DriverResult Editor(TextContentField field) {
            var model = BuildEditorViewModel(field);
            return ContentFieldTemplate(model, TemplateName, Prefix).Location("primary", "5");
        }

        protected override DriverResult Editor(TextContentField field, IUpdateModel updater) {
            var model = BuildEditorViewModel(field);
            updater.TryUpdateModel(model, Prefix, null, null);
            return ContentFieldTemplate(model, TemplateName, Prefix).Location("primary", "5");
        }

        private static TextContentFieldEditorViewModel BuildEditorViewModel(TextContentField field) {
            return new TextContentFieldEditorViewModel {
                TextContentField = field
            };
        }
    }
}