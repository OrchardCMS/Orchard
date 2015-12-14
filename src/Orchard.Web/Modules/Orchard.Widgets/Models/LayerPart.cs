using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Widgets.Models {
    public class LayerPart : ContentPart<LayerPartRecord> {
        
        /// <summary>
        /// The layer's name.
        /// </summary>
        [Required]
        public string Name {
            get { return Retrieve(x => x.Name); }
            set { Store(x => x.Name, value); }
        }

        /// <summary>
        /// The layer's description.
        /// </summary>
        public string Description {
            get { return Retrieve(x => x.Description); }
            set { Store(x => x.Description, value); }
        }

        /// <summary>
        /// The layer's rule. 
        /// The rule defines when the layer is active (should or not be displayed).
        /// </summary>
        public string LayerRule {
            get { return Retrieve(x => x.LayerRule); }
            set { Store(x => x.LayerRule, value); }
        }
    }
}