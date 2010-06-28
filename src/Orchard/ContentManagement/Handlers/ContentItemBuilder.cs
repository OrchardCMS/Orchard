using System.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Handlers {
    public class ContentItemBuilder {
        private readonly ContentTypeDefinition _definition;
        private readonly ContentItem _item;

        public ContentItemBuilder(ContentTypeDefinition definition) {
            _definition = definition;
            _item = new ContentItem {
                ContentType = definition.Name,
                TypeDefinition = definition
            };
        }

        public ContentItem Build() {
            return _item;
        }

        public ContentItemBuilder Weld<TPart>() where TPart : ContentPart, new() {
            var partName = typeof(TPart).Name;

            var typePartDefinition = _definition.Parts.FirstOrDefault(p => p.PartDefinition.Name == partName);
            if (typePartDefinition == null) {
                typePartDefinition = new ContentTypeDefinition.Part(
                    new ContentPartDefinition(partName),
                    new SettingsDictionary());

                var part = new TPart {
                    TypePartDefinition = typePartDefinition
                };
                _item.Weld(part);
            }
            return this;
        }

        public ContentItemBuilder Weld(ContentPart contentPart) {
            _item.Weld(contentPart);
            return this;
        }
    }
}
