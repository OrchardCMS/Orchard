using System.ComponentModel.DataAnnotations;

namespace Orchard.MediaProcessing.ViewModels {
    public class AdminCreateViewModel {
        [Required, StringLength(1024)]
        public string Name { get; set; }
    }
}