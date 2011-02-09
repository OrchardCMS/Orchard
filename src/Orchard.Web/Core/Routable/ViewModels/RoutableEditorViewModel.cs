using System.ComponentModel.DataAnnotations;

namespace Orchard.Core.Routable.ViewModels {
    public class RoutableEditorViewModel {

        public int Id { get; set; }
        public string ContentType { get; set; }

        [Required]
        [StringLength(1024)]
        public string Title { get; set; }
        [StringLength(1024)]
        public string Slug { get; set; }
        public int? ContainerId { get; set; }
        public bool PromoteToHomePage { get; set; }

        public string ContainerAbsoluteUrl { get; set; }
    }
}