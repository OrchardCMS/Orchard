using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Builders {
    public class ContentPartDefinitionBuilder {
        private string _name;
        private readonly IList<ContentPartDefinition.Field> _fields;
        private readonly SettingsDictionary _settings;

        public ContentPartDefinitionBuilder()
            : this(new ContentPartDefinition(null)) {
        }

        public ContentPartDefinitionBuilder(ContentPartDefinition existing) {
            if (existing == null) {
                _fields = new List<ContentPartDefinition.Field>();
                _settings = new SettingsDictionary();
            }
            else {
                _name = existing.Name;
                _fields = existing.Fields.ToList();
                _settings = new SettingsDictionary(existing.Settings.ToDictionary(kv => kv.Key, kv => kv.Value));
            }
        }

        public ContentPartDefinition Build() {
            return new ContentPartDefinition(_name, _fields, _settings);
        }

        public ContentPartDefinitionBuilder Named(string name) {
            _name = name;
            return this;
        }

        public ContentPartDefinitionBuilder RemoveField(string fieldName) {
            var existingField = _fields.SingleOrDefault(x => x.FieldDefinition.Name == fieldName);
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

        public ContentPartDefinitionBuilder WithField(string fieldName, Action<FieldConfigurer> configuration) {

            var existingField = _fields.FirstOrDefault(x => x.Name == fieldName);
            if (existingField != null) {
                var toRemove = _fields.Where(x => x.Name == fieldName).ToArray();
                foreach (var remove in toRemove) {
                    _fields.Remove(remove);
                }
            }
            else {
                existingField = new ContentPartDefinition.Field(fieldName);
            }
            var configurer = new FieldConfigurerImpl(existingField);
            configuration(configurer);
            _fields.Add(configurer.Build());
            return this;
        }

        public abstract class FieldConfigurer {
            protected readonly SettingsDictionary _settings;

            protected FieldConfigurer(ContentPartDefinition.Field field) {
                _settings = new SettingsDictionary(field.Settings.ToDictionary(kv => kv.Key, kv => kv.Value));
            }

            public FieldConfigurer WithSetting(string name, string value) {
                _settings[name] = value;
                return this;
            }

            public abstract FieldConfigurer OfType(ContentFieldDefinition fieldDefinition);
            public abstract FieldConfigurer OfType(string fieldType);
        }

        class FieldConfigurerImpl : FieldConfigurer {
            private ContentFieldDefinition _fieldDefinition;
            private readonly string _fieldName;

            public FieldConfigurerImpl(ContentPartDefinition.Field field)
                : base(field) {
                _fieldDefinition = field.FieldDefinition;
                _fieldName = field.Name;
            }

            public ContentPartDefinition.Field Build() {
                return new ContentPartDefinition.Field(_fieldDefinition, _fieldName, _settings);
            }

            public override FieldConfigurer OfType(ContentFieldDefinition fieldDefinition) {
                _fieldDefinition = fieldDefinition;
                return this;
            }

            public override FieldConfigurer OfType(string fieldType) {
                _fieldDefinition = new ContentFieldDefinition(fieldType);
                return this;
            }
        }

    }
}