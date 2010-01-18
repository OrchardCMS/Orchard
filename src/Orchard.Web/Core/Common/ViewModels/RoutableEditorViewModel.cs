using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class RoutableEditorViewModel {
        public RoutableAspect RoutableAspect { get; set; }

        [Required]
        public string Title {
            get { return RoutableAspect.Record.Title; }
            set { RoutableAspect.Record.Title = value; }
        }

        [RegularExpression(@"^[^/:?#\[\]@!$&'()*+,;=\s](?(?=/)/[^/:?#\[\]@!$&'()*+,;=\s]|[^:?#\[\]@!$&'()*+,;=\s])*$")]
        public string Slug {
            get { return RoutableAspect.Record.Slug; }
            set { RoutableAspect.Record.Slug = value; }
        }
    }
}