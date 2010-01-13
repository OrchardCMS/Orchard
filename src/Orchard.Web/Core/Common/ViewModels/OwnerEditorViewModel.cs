using System.ComponentModel.DataAnnotations;

namespace Orchard.Core.Common.ViewModels {
    public class OwnerEditorViewModel {
        [Required]
        public string Owner { get; set; }
    }
}