using System.Collections.Generic;

namespace Orchard.CmsPages.Services.Templates {
    public class TemplateDescriptor {
        public TemplateDescriptor() {
            Zones = new List<string>();
            Others = new List<MetadataEntry>();
        }

        public IList<string> Zones { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IList<MetadataEntry> Others { get; set; }
    }
}