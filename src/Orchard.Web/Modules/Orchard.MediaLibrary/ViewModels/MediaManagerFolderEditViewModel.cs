using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerFolderEditViewModel {
        [Required]
        public string Name { get; set; }
        public string FolderPath { get; set; }
    }
}
