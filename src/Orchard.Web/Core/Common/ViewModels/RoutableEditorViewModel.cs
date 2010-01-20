using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class RoutableEditorViewModel {
        public string Prefix { get; set; }
        public RoutableAspect RoutableAspect { get; set; }

        [Required]
        public string Title {
            get { return RoutableAspect.Record.Title; }
            set { RoutableAspect.Record.Title = value; }
        }

        public string Slug {
            get { return RoutableAspect.Record.Slug; }
            set { RoutableAspect.Record.Slug = value; }
        }
    }
}