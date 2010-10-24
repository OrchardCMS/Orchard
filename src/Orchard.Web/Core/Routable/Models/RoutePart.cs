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

        public string GetContainerPath() {
            var commonAspect = this.As<ICommonPart>();
            if (commonAspect != null && commonAspect.Container != null) {
                var routable = commonAspect.Container.As<IRoutableAspect>();
                if (routable != null) {
                    return routable.Path;
                }
            }
            return null;
        }

        public string GetPathWithSlug(string slug) {
            // TEMP: path format patterns replaces this logic
            var containerPath = GetContainerPath();
            if (string.IsNullOrEmpty(containerPath)) {
                return slug;
            }
            return containerPath + "/" + slug;
        }
    }
}