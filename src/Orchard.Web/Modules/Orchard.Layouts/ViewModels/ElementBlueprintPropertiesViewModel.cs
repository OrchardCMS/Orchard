using System.ComponentModel.DataAnnotations;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.ViewModels {
    public class ElementBlueprintPropertiesViewModel {
        public string ElementTypeName { get; set; }

        [Required]
        public string ElementDisplayName { get; set; }

        public string ElementCategory { get; set; }

        public IElement BaseElement { get; set; }
    }
}