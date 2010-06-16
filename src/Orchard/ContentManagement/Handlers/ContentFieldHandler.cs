using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Drivers;

namespace Orchard.ContentManagement.Handlers {
    public class ContentFieldHandler: ContentHandlerBase {
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;

        public ContentFieldHandler(IEnumerable<IContentFieldDriver> contentFieldDrivers) {
            _contentFieldDrivers = contentFieldDrivers;
        }

        public override void Activated(ActivatedContentContext context) {
            var fieldInfos = _contentFieldDrivers.SelectMany(x => x.GetFieldInfo());
            var parts = context.ContentItem.Parts;
            foreach (var contentPart in parts) {
                foreach (var field in contentPart.Fields) {
                    var fieldTypeName = field.FieldDefinition.Name;
                    var fieldInfo = fieldInfos.FirstOrDefault(x => x.FieldTypeName == fieldTypeName);
                    if (fieldInfo != null) 
                        contentPart.Weld(fieldInfo.Factory(field.PartFieldDefinition));
                }
            }
        }
    }
}
