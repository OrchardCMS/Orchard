using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuPartRecord : ContentPartRecord {
        public virtual string MenuText { get; set; }
        public virtual string MenuPosition { get; set; }
        public virtual bool OnMainMenu { get; set; }
    }
}