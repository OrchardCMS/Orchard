using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Models {
    public class ShapeMenuItemPart : ContentPart<ShapeMenuItemPartRecord> {
        /// <summary>
        /// Maximum number of items to retrieve from db
        /// </summary>
        public virtual string ShapeType {
            get { return Record.ShapeType; }
            set { Record.ShapeType = value; }
        }
    }
}