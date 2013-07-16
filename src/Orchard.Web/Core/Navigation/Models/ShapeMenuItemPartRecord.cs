using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class ShapeMenuItemPartRecord : ContentPartRecord {
        /// <summary>
        /// The shape to display
        /// </summary>
        public virtual string ShapeType { get; set; }
    }
}