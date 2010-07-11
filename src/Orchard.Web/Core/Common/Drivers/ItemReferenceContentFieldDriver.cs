using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class ItemReferenceContentFieldDriver : ContentFieldDriver<ItemReferenceContentField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Common.ItemReferenceContentField";

        public ItemReferenceContentFieldDriver(IOrchardServices services) {
            Services = services;
        }

        protected override string Prefix {
            get { return "ItemReferenceContentField"; }
        }

        protected override DriverResult Display(ItemReferenceContentField field, string displayType) {
            var model = new ItemReferenceContentFieldDisplayViewModel { 
                Item = Services.ContentManager.Get(field.ContentItemReference.Id)
            };

            return ContentFieldTemplate(model, TemplateName, Prefix);
        }

        protected override DriverResult Editor(ItemReferenceContentField field) {
            var model = BuildEditorViewModel(field);
            return ContentFieldTemplate(model, TemplateName, Prefix).Location("primary", "6");
        }

        protected override DriverResult Editor(ItemReferenceContentField field, IUpdateModel updater) {
            var model = BuildEditorViewModel(field);
            updater.TryUpdateModel(model, Prefix, null, null);
            return ContentFieldTemplate(model, TemplateName, Prefix).Location("primary", "6");
        }

        private ItemReferenceContentFieldEditorViewModel BuildEditorViewModel(ItemReferenceContentField field) {
            return new ItemReferenceContentFieldEditorViewModel {
                Item = Services.ContentManager.Get(field.ContentItemReference.Id)
            };
        }
    }
}