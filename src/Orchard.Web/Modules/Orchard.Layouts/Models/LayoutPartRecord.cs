using Orchard.ContentManagement.Records;

namespace Orchard.Layouts.Models {
    public class LayoutPartRecord : ContentPartVersionRecord {
        public virtual int? TemplateId { get; set; }
    }
}