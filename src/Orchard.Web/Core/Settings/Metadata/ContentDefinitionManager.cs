using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Settings.Metadata {
    public class ContentDefinitionManager : Component, IContentDefinitionManager {
        private readonly IRepository<ContentTypeDefinitionRecord> _typeDefinitionRepository;
        private readonly IRepository<ContentPartDefinitionRecord> _partDefinitionRepository;
        private readonly IMapper<XElement, IDictionary<string, string>> _settingsReader;
        private readonly IMapper<IDictionary<string, string>, XElement> _settingsWriter;

        public ContentDefinitionManager(
            IRepository<ContentTypeDefinitionRecord> typeDefinitionRepository,
            IRepository<ContentPartDefinitionRecord> partDefinitionRepository,
            IMapper<XElement, IDictionary<string, string>> settingsReader,
            IMapper<IDictionary<string, string>, XElement> settingsWriter) {
            _typeDefinitionRepository = typeDefinitionRepository;
            _partDefinitionRepository = partDefinitionRepository;
            _settingsReader = settingsReader;
            _settingsWriter = settingsWriter;
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            return _typeDefinitionRepository.Fetch(x => x.Name == name).Select(Build).SingleOrDefault();
        }

        public ContentPartDefinition GetPartDefinition(string name) {
            return _partDefinitionRepository.Fetch(x => x.Name == name).Select(Build).SingleOrDefault();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() {
            return _typeDefinitionRepository.Fetch(x => !x.Hidden).Select(Build).ToReadOnlyCollection();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions() {
            return _partDefinitionRepository.Fetch(x => !x.Hidden).Select(Build).ToReadOnlyCollection();
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            Apply(contentTypeDefinition, Acquire(contentTypeDefinition));
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition) {
            throw new NotImplementedException();
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition) {
            var result = _typeDefinitionRepository.Fetch(x => x.Name == contentTypeDefinition.Name).SingleOrDefault();
            if (result == null) {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name };
                _typeDefinitionRepository.Create(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition) {
            var result = _partDefinitionRepository.Fetch(x => x.Name == contentPartDefinition.Name).SingleOrDefault();
            if (result == null) {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name };
                _partDefinitionRepository.Create(result);
            }
            return result;
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record) {
            record.Settings = _settingsWriter.Map(model.Settings).ToString();

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(partDefinitionRecord => !model.Parts.Any(part => partDefinitionRecord.ContentPartDefinitionRecord.Name == part.PartDefinition.Name))
                .ToList();

            foreach (var remove in toRemove) {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts) {
                var partName = part.PartDefinition.Name;
                var typePartRecord = record.ContentTypePartDefinitionRecords.SingleOrDefault(r => r.ContentPartDefinitionRecord.Name == partName);
                if (typePartRecord == null) {
                    typePartRecord = new ContentTypePartDefinitionRecord { ContentPartDefinitionRecord = Acquire(part.PartDefinition) };
                    record.ContentTypePartDefinitionRecords.Add(typePartRecord);
                }
                Apply(part, typePartRecord);
            }
        }

        private void Apply(ContentTypeDefinition.Part model, ContentTypePartDefinitionRecord record) {
            record.Settings = Compose(_settingsWriter.Map(model.Settings));
        }



        ContentTypeDefinition Build(ContentTypeDefinitionRecord source) {
            return new ContentTypeDefinition(
                source.Name,
                source.ContentTypePartDefinitionRecords.Select(Build),
                _settingsReader.Map(Parse(source.Settings)));
        }

        ContentTypeDefinition.Part Build(ContentTypePartDefinitionRecord source) {
            return new ContentTypeDefinition.Part(
                Build(source.ContentPartDefinitionRecord),
                _settingsReader.Map(Parse(source.Settings)));
        }

        ContentPartDefinition Build(ContentPartDefinitionRecord source) {
            return new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                _settingsReader.Map(Parse(source.Settings)));
        }

        ContentPartDefinition.Field Build(ContentPartFieldDefinitionRecord source) {
            return new ContentPartDefinition.Field(
                Build(source.ContentFieldDefinitionRecord),
                source.Name,
                _settingsReader.Map(Parse(source.Settings)));
        }

        ContentFieldDefinition Build(ContentFieldDefinitionRecord source) {
            return new ContentFieldDefinition(source.Name);
        }

        XElement Parse(string settings) {
            if (string.IsNullOrEmpty(settings))
                return null;

            try {
                return XElement.Parse(settings);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Unable to parse settings xml");
                return null;
            }
        }
        string Compose(XElement map) {
            if (map == null)
                return null;

            return map.ToString();
        }
    }
}
