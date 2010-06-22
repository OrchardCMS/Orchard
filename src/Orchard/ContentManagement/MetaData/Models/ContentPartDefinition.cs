using System.Collections.Generic;
using System.Linq;
using Orchard.Utility.Extensions;

namespace Orchard.ContentManagement.MetaData.Models {
    public class ContentPartDefinition {
        public ContentPartDefinition(string name, IEnumerable<Field> fields, SettingsDictionary settings) {
            Name = name;
            Fields = fields.ToReadOnlyCollection();
            Settings = settings;
        }

        public ContentPartDefinition(string name) {  
            Name = name;
            Fields = Enumerable.Empty<Field>();
            Settings = new SettingsDictionary();
        }

        public string Name { get; private set; }
        public IEnumerable<Field> Fields { get; private set; }
        public SettingsDictionary Settings { get; private set; }

        public class Field {
            public Field(ContentFieldDefinition contentFieldDefinition, string name, SettingsDictionary settings) {
                FieldDefinition = contentFieldDefinition;
                Name = name;
                Settings = settings;
            }

            public string Name { get; private set; }
            public ContentFieldDefinition FieldDefinition { get; private set; }
            public SettingsDictionary Settings { get; private set; }
        }
    }
}
