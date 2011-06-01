using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Builders {
    public class ContentPartDefinitionBuilder {
        private string _name;
        private readonly IList<ContentPartFieldDefinition> _fields;
        private readonly SettingsDictionary _settings;

        public ContentPartDefinitionBuilder()
            : this(new ContentPartDefinition(null)) {
        }

        public ContentPartDefinitionBuilder(ContentPartDefinition existing) {
            if (existing == null) {
                _fields = new List<ContentPartFieldDefinition>();
                _settings = new SettingsDictionary();
            }
            else {
                _name = existing.Name;
                _fields = existing.Fields.ToList();
                _settings = new SettingsDictionary(existing.Settings.ToDictionary(kv => kv.Key, kv => kv.Value));
            }
        }

        public string Name { get { return _name; } }

        public ContentPartDefinition Build() {
            return new ContentPartDefinition(_name, _fields, _settings);
        }

        public ContentPartDefinitionBuilder Named(string name) {
            _name = name;
            return this;
        }

        public ContentPartDefinitionBuilder RemoveField(string fieldName) {
            var existingField = _fields.SingleOrDefault(x => x.Name == fieldName);
            if (existingField != null) {
                _fields.Remove(existingField);
            }
            return this;
        }

        public ContentPartDefinitionBuilder WithSetting(string name, string value) {
            _settings[name] = value;
            return this;
        }

        public ContentPartDefinitionBuilder WithField(string fieldName) {
            return WithField(fieldName, configuration => { });
        }

        public ContentPartDefinitionBuilder WithField(string fieldName, Action<ContentPartFieldDefinitionBuilder> configuration) {

            var existingField = _fields.FirstOrDefault(x => x.Name == fieldName);
            if (existingField != null) {
                var toRemove = _fields.Where(x => x.Name == fieldName).ToArray();
                foreach (var remove in toRemove) {
                    _fields.Remove(remove);
                }
            }
            else {
                existingField = new ContentPartFieldDefinition(fieldName);
            }
            var configurer = new FieldConfigurerImpl(existingField);
            configuration(configurer);
            _fields.Add(configurer.Build());
            return this;
        }

        class FieldConfigurerImpl : ContentPartFieldDefinitionBuilder {
            private ContentFieldDefinition _fieldDefinition;
            private readonly string _fieldName;

            public FieldConfigurerImpl(ContentPartFieldDefinition field)
                : base(field) {
                _fieldDefinition = field.FieldDefinition;
                _fieldName = field.Name;
            }

            public ContentPartFieldDefinition Build() {
                return new ContentPartFieldDefinition(_fieldDefinition, _fieldName, _settings);
            }

            public override string Name {
                get { return _fieldName; }
            }

            public override string FieldType {
                get { return _fieldDefinition.Name; }
            }

            public override ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition) {
                _fieldDefinition = fieldDefinition;
                return this;
            }

            public override ContentPartFieldDefinitionBuilder OfType(string fieldType) {
                _fieldDefinition = new ContentFieldDefinition(fieldType);
                return this;
            }
        }
    }

    public static class ContentPartDefinitionBuilderExtensions {
        public static IEnumerable<KeyValuePair<string, string>> GetSettingEntries(IDictionary<string, ContentLocation> locationSettings) {
            int entryIndex = 0;
            foreach (var entry in locationSettings) {
                var zone = string.IsNullOrEmpty(entry.Value.Zone) ? null : entry.Value.Zone;
                var position = string.IsNullOrEmpty(entry.Value.Position) ? null : entry.Value.Position;
                var locationName = (zone == null && position == null) ? null : entry.Key;

                if (locationName != null) {
                    var prefix = string.Format("LocationSettings[{0}]", entryIndex);
                    yield return new KeyValuePair<string, string>(string.Format("{0}.Key", prefix), locationName);
                    yield return new KeyValuePair<string, string>(string.Format("{0}.Value.Zone", prefix), zone);
                    yield return new KeyValuePair<string, string>(string.Format("{0}.Value.Position", prefix), position);

                    entryIndex++;
                }
            }

            // Clear the remaining entries from [index -> end of collection]
            for (int i = entryIndex; i < locationSettings.Count; i++) {
                var prefix = string.Format("LocationSettings[{0}]", i);
                yield return new KeyValuePair<string, string>(string.Format("{0}.Key", prefix), null);
                yield return new KeyValuePair<string, string>(string.Format("{0}.Value.Zone", prefix), null);
                yield return new KeyValuePair<string, string>(string.Format("{0}.Value.Position", prefix), null);
            }
        }

        public static ContentPartDefinitionBuilder WithLocation(this ContentPartDefinitionBuilder obj, IDictionary<string, ContentLocation> settings) {
            foreach (var entry in GetSettingEntries(settings))
                obj = obj.WithSetting(entry.Key, entry.Value);
            return obj;
        }

        public static ContentTypePartDefinitionBuilder WithLocation(this ContentTypePartDefinitionBuilder obj, IDictionary<string, ContentLocation> settings) {
            foreach (var entry in GetSettingEntries(settings))
                obj = obj.WithSetting(entry.Key, entry.Value);
            return obj;
        }

        public static ContentPartFieldDefinitionBuilder WithLocation(this ContentPartFieldDefinitionBuilder obj, IDictionary<string, ContentLocation> settings) {
            foreach (var entry in GetSettingEntries(settings))
                obj = obj.WithSetting(entry.Key, entry.Value);
            return obj;
        }
    }
}