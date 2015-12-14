using System.ComponentModel.DataAnnotations;

namespace Orchard.Projections.ViewModels {
    public class BindingEditViewModel {
        public int Id { get; set; }

        [StringLength(255), Required]
        public string FullName { get; set; }

        [StringLength(64), Required]
        public string Member { get; set; }

        [StringLength(64), Required]
        public string Display { get; set; }

        [StringLength(500), Required]
        public string Description { get; set; }
    }
}
