using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Models {
    public class ShapeMenuItemPart : ContentPart {
        /// <summary>
        /// Maximum number of items to retrieve from db
        /// </summary>
        public string ShapeType {
            get { return this.Retrieve(x => x.ShapeType); }
            set { this.Store(x => x.ShapeType, value); }
        }
    }
}