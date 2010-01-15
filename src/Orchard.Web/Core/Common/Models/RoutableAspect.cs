using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class RoutableAspect : ContentPart<RoutableRecord> {
        public string Title {
            get { return Record.Title; }
            set { Record.Title = value; }
        }

        [Required]
        public string Slug {
            get { return Record.Slug; }
            set { Record.Slug = value; }
        }
    }
}