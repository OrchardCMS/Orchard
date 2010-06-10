using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Core.Common.Models {
    public class RoutableAspect : ContentPart<RoutableRecord>, IRoutableAspect {
        public string ContentItemBasePath { get; set; }

        public string Title {
            get { return Record.Title; }
            set { Record.Title = value; }
        }

        public string Slug {
            get { return Record.Slug; }
            set { Record.Slug = value; }
        }
    }
}
