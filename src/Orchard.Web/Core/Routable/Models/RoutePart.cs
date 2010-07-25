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

        public string GetContainerSlug() {
            var commonAspect = this.As<ICommonPart>();
            if (commonAspect != null && commonAspect.Container != null) {
                var routable = commonAspect.Container.As<IRoutableAspect>();
                if (routable != null) {
                    return routable.Slug;
                }
            }
            return null;
        }

        public string GetPathFromSlug(string slug) {
            // TEMP: path format patterns replaces this logic
            var containerSlug = GetContainerSlug();
            if (string.IsNullOrEmpty(containerSlug)) {
                return slug;
            }
            return containerSlug + "/" + slug;
        }
    }
}