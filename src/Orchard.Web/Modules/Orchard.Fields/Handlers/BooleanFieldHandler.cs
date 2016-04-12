using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using System;
using System.Linq;

namespace Orchard.Fields.Handlers {
    public class BooleanFieldHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public BooleanFieldHandler(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override void Initializing(InitializingContentContext context) {
            base.Initializing(context);

            var fields = context.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(BooleanField).Name)).Cast<BooleanField>();

            // define lazy initializer for ContentPickerField.ContentItems
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null) {
                return;
            }

            foreach (var field in fields) {
                var localField = field;
                field._valueField.Loader(() => field.PartFieldDefinition.Settings.GetModel<BooleanFieldSettings>().DefaultValue);
                field._valueField.Setter((value) => {
                    field.Storage.Set(value);
                    return value;
                });
            }
        }

        protected override void Loading(LoadContentContext context) {
            base.Loading(context);

            var fields = context.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(BooleanField).Name)).Cast<BooleanField>();

            // define lazy initializer for ContentPickerField.ContentItems
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null) {
                return;
            }

            foreach (var field in fields) {
                var localField = field;
                field._valueField.Loader(() => field.Storage.Get<Boolean?>());
            }
        }
    }
}