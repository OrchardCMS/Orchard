using System;
using System.Runtime.Serialization;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Security {
    [Serializable]
    public class OrchardSecurityException : OrchardCoreException {
        public OrchardSecurityException(LocalizedString message) : base(message) { }
        public OrchardSecurityException(LocalizedString message, Exception innerException) : base(message, innerException) { }
        protected OrchardSecurityException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public string PermissionName { get; set; }
        public IUser User { get; set; }
        public IContent Content { get; set; }
    }
}
