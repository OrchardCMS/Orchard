using System;
using Orchard.Localization;

namespace Orchard.ContentManagement.Handlers {
    public class DescribeMembersContext {
        private readonly Action<string, Type, LocalizedString, LocalizedString> _processMember;

        public DescribeMembersContext(Action<string, Type, LocalizedString, LocalizedString> processMember) {
            _processMember = processMember;
        }

        public DescribeMembersContext Member(string storageName, Type storageType) {
            return Member(storageName, storageType, null);
        }

        public DescribeMembersContext Member(string storageName, Type storageType, LocalizedString displayName) {
            return Member(storageName, storageType, displayName, null);
        }

        public DescribeMembersContext Member(string storageName, Type storageType, LocalizedString displayName, LocalizedString description) {
            _processMember(storageName, storageType, displayName, description);
            return this;
        }
    }
}