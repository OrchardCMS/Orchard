using System;
using System.Runtime.Serialization;

namespace Orchard.Security {
    public class OrchardSecurityException : OrchardException {
        public OrchardSecurityException() : base("A security exception occurred in the content management system.") { }
        public OrchardSecurityException(Exception innerException) : base(innerException.Message, innerException) { }
        public OrchardSecurityException(string message) : base(message) { }
        protected OrchardSecurityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public OrchardSecurityException(string message, Exception innerException) : base(message, innerException) { }

        public string PermissionName { get; set; }
    }
}
