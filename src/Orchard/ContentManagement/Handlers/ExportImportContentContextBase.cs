using System.Xml.Linq;

namespace Orchard.ContentManagement.Handlers {
    public class ExportImportContentContextBase : ContentContextBase {
        public XElement Data { get; set; }
        public string FieldName { get; set; } //in case we are processing fields, this is the name of the field. The actual name, not the name of the type.
        public ExportImportContentContextBase(ContentItem contentItem) : base(contentItem) { }
    }
}
