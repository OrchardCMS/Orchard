using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Media.ViewModels {
    public class MediaFolderCreateViewModel : BaseViewModel {
        [Required, DisplayName("Folder Name:")]
        public string Name { get; set; }
        public string MediaPath { get; set; }
    }
}
