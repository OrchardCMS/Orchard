using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Orchard.ContentManagement.MetaData.Models {
    public class ContentTypeDefinition {
        public ContentTypeDefinition(string name, string displayName, IEnumerable<Part> parts, SettingsDictionary settings) {
            Name = name;
            DisplayName = displayName;
            Parts = parts;
            Settings = settings;
        }

        public ContentTypeDefinition(string name, string displayName) {
            Name = name;
            DisplayName = displayName;
            Parts = Enumerable.Empty<Part>();
            Settings = new SettingsDictionary();
        }

        [StringLength(128)]
        public string Name { get; private set; }
        [Required, StringLength(1024)]
        public string DisplayName { get; private set; }
        public IEnumerable<Part> Parts { get; private set; }
        public SettingsDictionary Settings { get; private set; }

        public class Part {
    
            public Part(ContentPartDefinition contentPartDefinition, SettingsDictionary settings) {
                PartDefinition = contentPartDefinition;
                Settings = settings;
            }

            public Part() {
                Settings = new SettingsDictionary();
            }

            public ContentPartDefinition PartDefinition { get; private set; }
            public SettingsDictionary Settings { get; private set; }
        }
    }
}
