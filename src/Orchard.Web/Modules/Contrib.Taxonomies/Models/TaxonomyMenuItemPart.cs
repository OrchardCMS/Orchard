using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Contrib.Taxonomies.Models {
    public class TaxonomyMenuItemPart : ContentPart<TaxonomyMenuItemPartRecord> {
        public bool RenderMenuItem {
            get { return Record.RenderMenuItem; }
            set { Record.RenderMenuItem = value; }
        }
                
        [StringLength(30)]
        public string Position {
            get { return Record.Position; }
            set { Record.Position = value; }
        }

        [StringLength(255)]
        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

    }
}
