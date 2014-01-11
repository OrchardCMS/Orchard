using System.ComponentModel.DataAnnotations;

namespace Orchard.Templates.ViewModels {
    public class ShapePartViewModel {
        [StringLength(100)]
        public string Name { get; set; }
        public string Template { get; set; }
    }
}