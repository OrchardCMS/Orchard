using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Widgets.Models {
    public class LayerPart : ContentPart<LayerPartRecord> {
        
        /// <summary>
        /// The layer's name.
        /// </summary>
        [Required]
        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        /// <summary>
        /// The layer's description.
        /// </summary>
        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        /// <summary>
        /// The layer's rule. 
        /// The rule defines when the layer is active (should or not be displayed).
        /// </summary>
        public string LayerRule {
            get { return Record.LayerRule; }
            set { Record.LayerRule = value; }
        }
    }
}