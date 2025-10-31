using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement
{
    public class ContentField
    {
        public string Name => PartFieldDefinition.Name;
        public string DisplayName => PartFieldDefinition.DisplayName;

        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
        public ContentFieldDefinition FieldDefinition => PartFieldDefinition.FieldDefinition;

        public IFieldStorage Storage { get; set; }
    }
}
