using System;
using System.Runtime.Serialization;

namespace Orchard {
    public class OrchardException : ApplicationException {
        public OrchardException() : base("An exception occurred in the content management system.") { }
        public OrchardException(Exception innerException) : base(innerException.Message, innerException) { }
        public OrchardException(string message) : base(message) { }
        protected OrchardException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public OrchardException(string message, Exception innerException) : base(message, innerException) { }
    }
}
