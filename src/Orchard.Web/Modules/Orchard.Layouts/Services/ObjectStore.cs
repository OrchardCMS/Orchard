using System;
using Newtonsoft.Json;
using Orchard.Data;
using Orchard.Layouts.Models;
using Orchard.Services;
using Orchard.Validation;

namespace Orchard.Layouts.Services {
    public class ObjectStore : IObjectStore {
        private readonly IRepository<ObjectStoreEntry> _repository;
        private readonly IClock _clock;
        private readonly IWorkContextAccessor _wca;
        private readonly Lazy<int> _currentUserIdField;

        public ObjectStore(IRepository<ObjectStoreEntry> repository, IClock clock, IWorkContextAccessor wca) {
            _repository = repository;
            _clock = clock;
            _wca = wca;

            _currentUserIdField = new Lazy<int>(() => {
                var currentUser = _wca.GetContext().CurrentUser;
                return currentUser != null ? currentUser.Id : 0;
            });
        }

        private int CurrentUserId {
            get { return _currentUserIdField.Value; }
        }

        public ObjectStoreEntry GetEntry(string key) {
            Argument.ThrowIfNull(key, "key");
            return _repository.Get(x => x.EntryKey == key && x.UserId == CurrentUserId);
        }

        public ObjectStoreEntry GetOrCreateEntry(string key) {
            Argument.ThrowIfNull(key, "key");

            var entry = GetEntry(key);

            if (entry == null) {
                entry = new ObjectStoreEntry {
                    EntryKey = key,
                    UserId = CurrentUserId,
                    CreatedUtc = _clock.UtcNow,
                    LastModifiedUtc = _clock.UtcNow
                };

                _repository.Create(entry);
            }

            return entry;
        }

        public ObjectStoreEntry GetOrCreateEntry() {
            var key = GenerateKey();
            return GetOrCreateEntry(key);
        }

        public void Set<T>(string key, T value) {
            Argument.ThrowIfNull(key, "key");

            var entry = GetOrCreateEntry(key);
            var json = JsonConvert.SerializeObject(value);

            entry.Data = json;
            entry.LastModifiedUtc = _clock.UtcNow;
        }

        public string Set<T>(T value) {
            var key = GenerateKey();
            Set(key, value);
            return key;
        }

        public T Get<T>(string key, Func<T> defaultValue = null) {
            Argument.ThrowIfNull(key, "key");

            var entry = GetOrCreateEntry(key);
            var json = entry.Data;

            if (String.IsNullOrWhiteSpace(json))
                return defaultValue != null ? defaultValue() : default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public string GenerateKey() {
            return Guid.NewGuid().ToString();
        }

        public bool Remove(string key) {
            var entry = GetEntry(key);

            if (entry == null) {
                return false;
            }

            _repository.Delete(entry);
            return true;
        }
    }
}