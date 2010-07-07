using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Builders {
    public class ContentTypeDefinitionBuilder {
        private string _name;
        private string _displayName;
        private readonly IList<ContentTypeDefinition.Part> _parts;
        private readonly SettingsDictionary _settings;

        public ContentTypeDefinitionBuilder()
            : this(new ContentTypeDefinition(null, null)) {
        }

        public ContentTypeDefinitionBuilder(ContentTypeDefinition existing) {
            if (existing == null) {
                _parts = new List<ContentTypeDefinition.Part>();
                _settings = new SettingsDictionary();
            }
            else {
                _name = existing.Name;
                _displayName = existing.DisplayName;
                _parts = existing.Parts.ToList();
                _settings = new SettingsDictionary(existing.Settings.ToDictionary(kv => kv.Key, kv => kv.Value));
            }
        }

        private void Init(ContentTypeDefinition existing) {

        }

        public ContentTypeDefinition Build() {
            return new ContentTypeDefinition(_name, _displayName, _parts, _settings);
        }

        public ContentTypeDefinitionBuilder Named(string name) {
            _name = name;
            return this;
        }

        public ContentTypeDefinitionBuilder DisplayedAs(string displayName) {
            _displayName = displayName;
            return this;
        }

        public ContentTypeDefinitionBuilder WithSetting(string name, string value) {
            _settings[name] = value;
            return this;
        }

        public ContentTypeDefinitionBuilder RemovePart(string partName) {
            var existingPart = _parts.SingleOrDefault(x => x.PartDefinition.Name == partName);
            if (existingPart != null) {
                _parts.Remove(existingPart);
            }
            return this;
        }

        public ContentTypeDefinitionBuilder WithPart(string partName) {
            return WithPart(partName, configuration => { });
        }

        public ContentTypeDefinitionBuilder WithPart(string partName, Action<PartConfigurer> configuration) {
            return WithPart(new ContentPartDefinition(partName), configuration);
        }

        public ContentTypeDefinitionBuilder WithPart(ContentPartDefinition partDefinition, Action<PartConfigurer> configuration) {
            var existingPart = _parts.SingleOrDefault(x => x.PartDefinition.Name == partDefinition.Name);
            if (existingPart != null) {
                _parts.Remove(existingPart);
            }
            else {
                existingPart = new ContentTypeDefinition.Part(partDefinition, new SettingsDictionary());
            }
            var configurer = new PartConfigurerImpl(existingPart);
            configuration(configurer);
            _parts.Add(configurer.Build());
            return this;
        }

        public abstract class PartConfigurer {
            protected readonly SettingsDictionary _settings;

            protected PartConfigurer(ContentTypeDefinition.Part part) {
                Name = part.PartDefinition.Name;
                _settings = new SettingsDictionary(part.Settings.ToDictionary(kv => kv.Key, kv => kv.Value));
            }

            public string Name { get; private set; }

            public PartConfigurer WithSetting(string name, string value) {
                _settings[name] = value;
                return this;
            }
        }

        class PartConfigurerImpl : PartConfigurer {
            private readonly ContentPartDefinition _partDefinition;

            public PartConfigurerImpl(ContentTypeDefinition.Part part)
                : base(part) {
                _partDefinition = part.PartDefinition;
            }

            public ContentTypeDefinition.Part Build() {
                return new ContentTypeDefinition.Part(_partDefinition, _settings);
            }
        }

    }
}
