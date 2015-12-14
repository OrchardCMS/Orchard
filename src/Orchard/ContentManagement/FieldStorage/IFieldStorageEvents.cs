using System;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Events;

namespace Orchard.ContentManagement.FieldStorage {
    public class FieldStorageEventContext {
        public IContent Content { get; set; }
        public string PartName { get; set; }
        public string FieldName { get; set; }
        public string ValueName { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }
    }

    public interface IFieldStorageEvents : IEventHandler {
        void SetCalled(FieldStorageEventContext context);
    }

    public class FieldStorageEventStorage : IFieldStorage {
        private readonly IFieldStorage _concreteStorage;
        private readonly ContentPartFieldDefinition _contentPartFieldDefinition;
        private readonly ContentPart _contentPart;
        private readonly IEnumerable<IFieldStorageEvents> _events;

        public FieldStorageEventStorage(
            IFieldStorage concreteStorage,
            ContentPartFieldDefinition contentPartFieldDefinition,
            ContentPart contentPart,
            IEnumerable<IFieldStorageEvents> events) {
            _concreteStorage = concreteStorage;
            _contentPartFieldDefinition = contentPartFieldDefinition;
            _contentPart = contentPart;
            _events = events;
        }

        public T Get<T>(string name) {
            return _concreteStorage.Get<T>(name);
        }

        public void Set<T>(string name, T value) {
            _concreteStorage.Set(name, value);

            var context = new FieldStorageEventContext {
                FieldName = _contentPartFieldDefinition.Name,
                PartName = _contentPart.PartDefinition.Name,
                Value = value,
                ValueName = name,
                ValueType = typeof (T),
                Content = _contentPart
            };

            foreach (var fieldEvent in _events) {
                fieldEvent.SetCalled(context);
            }
        }
    }
}
