using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.MetaData.Models {
    public class ContentTypeDefinition {
        public ContentTypeDefinition(string name, IEnumerable<Part> parts, IDictionary<string, string> settings) {
            Name = name;
            Parts = parts;
            Settings = settings;
        }

        public ContentTypeDefinition(string name) {
            Name = name;
            Parts = Enumerable.Empty<Part>();
            Settings = new Dictionary<string, string>();
        }

        public string Name { get; private set; }
        public IEnumerable<Part> Parts { get; private set; }
        public IDictionary<string, string> Settings { get; private set; }

        public class Part {
            public Part(ContentPartDefinition contentPartDefinition, IDictionary<string, string> settings) {
                PartDefinition = contentPartDefinition;
                Settings = settings;
            }

            public ContentPartDefinition PartDefinition { get; private set; }
            public IDictionary<string, string> Settings { get; private set; }
        }
    }
}
