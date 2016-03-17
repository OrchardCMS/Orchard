using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Exceptions;

namespace Orchard.Core.Settings.Metadata {
    public class ContentDefinitionManager : Component, IContentDefinitionManager {
        private const string ContentDefinitionSignal = "ContentDefinitionManager";
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IRepository<ContentTypeDefinitionRecord> _typeDefinitionRepository;
        private readonly IRepository<ContentPartDefinitionRecord> _partDefinitionRepository;
        private readonly IRepository<ContentFieldDefinitionRecord> _fieldDefinitionRepository;
        private readonly ISettingsFormatter _settingsFormatter;

        public ContentDefinitionManager(
            ICacheManager cacheManager,
            ISignals signals,
            IRepository<ContentTypeDefinitionRecord> typeDefinitionRepository,
            IRepository<ContentPartDefinitionRecord> partDefinitionRepository,
            IRepository<ContentFieldDefinitionRecord> fieldDefinitionRepository,
            ISettingsFormatter settingsFormatter) {
            _cacheManager = cacheManager;
            _signals = signals;
            _typeDefinitionRepository = typeDefinitionRepository;
            _partDefinitionRepository = partDefinitionRepository;
            _fieldDefinitionRepository = fieldDefinitionRepository;
            _settingsFormatter = settingsFormatter;
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            if (String.IsNullOrWhiteSpace(name)) {
                return null;
            }

            var contentTypeDefinitions = AcquireContentTypeDefinitions();
            if (contentTypeDefinitions.ContainsKey(name)) {
                return contentTypeDefinitions[name];
            }

            return null;
        }

        public void DeleteTypeDefinition(string name) {
            var record = _typeDefinitionRepository.Table.SingleOrDefault(x => x.Name == name);

            // deletes the content type record associated
            if (record != null) {
                _typeDefinitionRepository.Delete(record);
            }

            // invalidates the cache
            TriggerContentDefinitionSignal();
        }

        public void DeletePartDefinition(string name) {
            // remove parts from current types
            var typesWithPart = ListTypeDefinitions().Where(typeDefinition => typeDefinition.Parts.Any(part => part.PartDefinition.Name == name));

            foreach (var typeDefinition in typesWithPart) {
                this.AlterTypeDefinition(typeDefinition.Name, builder => builder.RemovePart(name));
            }

            // delete part
            var record = _partDefinitionRepository.Table.SingleOrDefault(x => x.Name == name);

            if (record != null) {
                _partDefinitionRepository.Delete(record);
            }

            // invalidates the cache
            TriggerContentDefinitionSignal();

        }

        public ContentPartDefinition GetPartDefinition(string name) {
            if (String.IsNullOrWhiteSpace(name)) {
                return null;
            }

            var contentPartDefinitions = AcquireContentPartDefinitions();
            if (contentPartDefinitions.ContainsKey(name)) {
                return contentPartDefinitions[name];
            }

            return null;
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() {
            return AcquireContentTypeDefinitions().Values;
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions() {
            return AcquireContentPartDefinitions().Values;
        }

        public IEnumerable<ContentFieldDefinition> ListFieldDefinitions() {
            return AcquireContentFieldDefinitions().Values;
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            Apply(contentTypeDefinition, Acquire(contentTypeDefinition));
            TriggerContentDefinitionSignal();
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition) {
            Apply(contentPartDefinition, Acquire(contentPartDefinition));
            TriggerContentDefinitionSignal();
        }

        private void MonitorContentDefinitionSignal(AcquireContext<string> ctx) {
            ctx.Monitor(_signals.When(ContentDefinitionSignal));
        }

        private void TriggerContentDefinitionSignal() {
            _signals.Trigger(ContentDefinitionSignal);
        }

        private IDictionary<string, ContentTypeDefinition> AcquireContentTypeDefinitions() {
            return _cacheManager.Get("ContentTypeDefinitions", true, ctx => {
                MonitorContentDefinitionSignal(ctx);

                AcquireContentPartDefinitions();

                var contentTypeDefinitionRecords = _typeDefinitionRepository.Table
                    .FetchMany(x => x.ContentTypePartDefinitionRecords)
                    .ThenFetch(x => x.ContentPartDefinitionRecord)
                    .Select(Build);

                return contentTypeDefinitionRecords.ToDictionary(x => x.Name, y => y, StringComparer.OrdinalIgnoreCase);
            });
        }

        private IDictionary<string, ContentPartDefinition> AcquireContentPartDefinitions() {
            return _cacheManager.Get("ContentPartDefinitions", true, ctx => {
                MonitorContentDefinitionSignal(ctx);

                var contentPartDefinitionRecords = _partDefinitionRepository.Table
                    .FetchMany(x => x.ContentPartFieldDefinitionRecords)
                    .ThenFetch(x => x.ContentFieldDefinitionRecord)
                    .Select(Build);

                return contentPartDefinitionRecords.ToDictionary(x => x.Name, y => y, StringComparer.OrdinalIgnoreCase);
            });
        }

        private IDictionary<string, ContentFieldDefinition> AcquireContentFieldDefinitions() {
            return _cacheManager.Get("ContentFieldDefinitions", true, ctx => {
                MonitorContentDefinitionSignal(ctx);

                return _fieldDefinitionRepository.Table.Select(Build).ToDictionary(x => x.Name, y => y);
            });
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition) {
            var result = _typeDefinitionRepository.Table.SingleOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null) {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                _typeDefinitionRepository.Create(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition) {
            var result = _partDefinitionRepository.Table.SingleOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null) {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name };
                _partDefinitionRepository.Create(result);
            }
            return result;
        }

        private ContentFieldDefinitionRecord Acquire(ContentFieldDefinition contentFieldDefinition) {
            var result = _fieldDefinitionRepository.Table.SingleOrDefault(x => x.Name == contentFieldDefinition.Name);
            if (result == null) {
                result = new ContentFieldDefinitionRecord { Name = contentFieldDefinition.Name };
                _fieldDefinitionRepository.Create(result);
            }
            return result;
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record) {
            record.DisplayName = model.DisplayName;
            record.Settings = _settingsFormatter.Map(model.Settings).ToString();

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(partDefinitionRecord => model.Parts.All(part => partDefinitionRecord.ContentPartDefinitionRecord.Name != part.PartDefinition.Name))
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

        private void Apply(ContentTypePartDefinition model, ContentTypePartDefinitionRecord record) {
            record.Settings = Compose(_settingsFormatter.Map(model.Settings));
        }

        private void Apply(ContentPartDefinition model, ContentPartDefinitionRecord record) {
            record.Settings = _settingsFormatter.Map(model.Settings).ToString();

            var toRemove = record.ContentPartFieldDefinitionRecords
                .Where(partFieldDefinitionRecord => model.Fields.All(partField => partFieldDefinitionRecord.Name != partField.Name))
                .ToList();

            foreach (var remove in toRemove) {
                record.ContentPartFieldDefinitionRecords.Remove(remove);
            }

            foreach (var field in model.Fields) {
                var fieldName = field.Name;
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.SingleOrDefault(r => r.Name == fieldName);
                if (partFieldRecord == null) {
                    partFieldRecord = new ContentPartFieldDefinitionRecord {
                        ContentFieldDefinitionRecord = Acquire(field.FieldDefinition),
                        Name = field.Name
                    };
                    record.ContentPartFieldDefinitionRecords.Add(partFieldRecord);
                }
                Apply(field, partFieldRecord);
            }
        }

        private void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionRecord record) {
            record.Settings = Compose(_settingsFormatter.Map(model.Settings));
        }

        ContentTypeDefinition Build(ContentTypeDefinitionRecord source) {
            return new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(Build),
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source) {
            return new ContentTypePartDefinition(
                Build(source.ContentPartDefinitionRecord),
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentPartDefinition Build(ContentPartDefinitionRecord source) {
            return new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source) {
            return new ContentPartFieldDefinition(
                Build(source.ContentFieldDefinitionRecord),
                source.Name,
                _settingsFormatter.Map(Parse(source.Settings)));
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
                if (ex.IsFatal()) {                
                    throw;
                }
                Logger.Error(ex, "Unable to parse settings xml");
                return null;
            }
        }

        static string Compose(XElement map) {
            if (map == null)
                return null;

            return map.ToString();
        }
    }

}
