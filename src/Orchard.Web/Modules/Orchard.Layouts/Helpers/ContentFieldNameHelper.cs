using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.Layouts.Helpers {
    public static class ContentFieldHelper {
        public static void GetPartAndFieldName(this string elementTypeName, out string partName, out string fieldName) {
            var typeNameParts = elementTypeName.Split(new[] { '.' });
            partName = typeNameParts[0];
            fieldName = typeNameParts[1];
        }

        public static ContentField GetContentField(this ContentItem contentItem, string elementTypeName) {
            string partName, fieldName;
            GetPartAndFieldName(elementTypeName, out partName, out fieldName);
            return contentItem.Parts.Single(x => x.PartDefinition.Name == partName).Fields.Single(x => x.Name == fieldName);
        }
    }
}