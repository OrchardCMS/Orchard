using System.ComponentModel.DataAnnotations;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.MediaLibrary.MediaFileName
{
    public class MediaFileNameEditorViewModel : Shape {
        [Required]
        public string FileName { get; set; }
    }
}