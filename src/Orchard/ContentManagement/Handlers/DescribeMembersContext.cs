using System;
using System.Collections;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Localization;

namespace Orchard.ContentManagement.Handlers {
    public class DescribeMembersContext {
        private readonly Action<string, Type, LocalizedString, LocalizedString> _processMember;
        private readonly IFieldStorage _storage;
        private readonly Action<IEnumerable> _apply;

        public DescribeMembersContext(Action<string, Type, LocalizedString, LocalizedString> processMember) : this(processMember, null, null) {
        }

        public DescribeMembersContext(IFieldStorage storage, Action<IEnumerable> apply)
            : this(null, storage, apply) {
        }

        public DescribeMembersContext(Action<string, Type, LocalizedString, LocalizedString> processMember, IFieldStorage storage, Action<IEnumerable> apply) {
            _processMember = processMember;
            _storage = storage;
            _apply = apply;
        }

        public DescribeMembersContext Member(string storageName, Type storageType) {
            return Member(storageName, storageType, null);
        }

        public DescribeMembersContext Member(string storageName, Type storageType, LocalizedString displayName) {
            return Member(storageName, storageType, displayName, null);
        }

        public DescribeMembersContext Member(string storageName, Type storageType, LocalizedString displayName, LocalizedString description) {
            if (_processMember != null) {
                _processMember(storageName, storageType, displayName, description);
            }
            return this;
        }

        public DescribeMembersContext Enumerate<TField>(Func<Func<TField, IEnumerable>> enumerate) where TField : ContentField, new() {
            
            if (enumerate != null && _storage != null && _apply != null) {
                var f = enumerate();
                var field = Activator.CreateInstance<TField>();
                field.Storage = _storage;
                _apply(f(field));
            }

            return this;
        }
    }
}