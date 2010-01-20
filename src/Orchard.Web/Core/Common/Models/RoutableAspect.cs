using Orchard.Core.Common.Records;
using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class RoutableAspect : ContentPart<RoutableRecord> {
        public string ContainerPath { get; set; }

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