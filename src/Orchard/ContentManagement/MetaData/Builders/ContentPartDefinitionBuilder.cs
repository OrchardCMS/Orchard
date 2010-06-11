using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Builders {
    public class ContentPartDefinitionBuilder {
        private readonly string _name;
        private readonly IList<ContentPartDefinition.Field> _fields;
        private readonly IDictionary<string, string> _settings;

        public ContentPartDefinitionBuilder()
            : this(new ContentPartDefinition(null)) {
        }

        public ContentPartDefinitionBuilder(ContentPartDefinition existing) {
            if (existing == null) {
                _fields = new List<ContentPartDefinition.Field>();
                _settings = new Dictionary<string, string>();
            }
            else {
                _name = existing.Name;
                _fields = existing.Fields.ToList();
                _settings = existing.Settings.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public ContentPartDefinition Build() {
            return new ContentPartDefinition(_name, _fields, _settings);
        }

        public ContentPartDefinitionBuilder WithSetting(string name, string value) {
            _settings[name] = value;
            return this;
        }

    }
}