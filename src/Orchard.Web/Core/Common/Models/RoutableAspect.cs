using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class RoutableAspect : ContentPart<RoutableRecord> {
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