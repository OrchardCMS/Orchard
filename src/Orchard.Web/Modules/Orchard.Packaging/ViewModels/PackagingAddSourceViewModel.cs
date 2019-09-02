using System.ComponentModel.DataAnnotations;

namespace Orchard.Packaging.ViewModels {
    public class PackagingAddSourceViewModel {

        [Required]
        public string Url { get; set; }
    }
}