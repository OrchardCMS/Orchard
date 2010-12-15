using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Core.Routable.Models {
    public class RoutePart : ContentPart<RoutePartRecord>, IRoutableAspect {
        public string Title {
            get { return Record.Title; }
            set { Record.Title = value; }
        }

        public string Slug {
            get { return Record.Slug; }
            set { Record.Slug = value; }
        }

        public string Path {
            get { return Record.Path; }
            set { Record.Path = value; }
        }

        public bool PromoteToHomePage { get; set; }
    }
}