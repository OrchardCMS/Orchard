using System.ComponentModel.DataAnnotations;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Core.Common.OwnerEditor {
    public class OwnerEditorViewModel : Shape {
        [Required]
        public string Owner { get; set; }
    }
}