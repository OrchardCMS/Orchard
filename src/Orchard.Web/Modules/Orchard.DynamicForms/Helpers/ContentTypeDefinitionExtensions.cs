using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.DynamicForms.Helpers {
    public static class ContentTypeDefinitionExtensions {
        public static string Stereotype(this ContentTypeDefinition contentTypeDefinition) {
            return contentTypeDefinition.Settings.ContainsKey("Stereotype") ? contentTypeDefinition.Settings["Stereotype"] : null;
        }
    }
}