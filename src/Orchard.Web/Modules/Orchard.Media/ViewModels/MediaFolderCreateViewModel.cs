using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Media.ViewModels {
    public class MediaFolderCreateViewModel {
        [Required, DisplayName("Folder Name:")]
        public string Name { get; set; }
        public string MediaPath { get; set; }
    }
}
