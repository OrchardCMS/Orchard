using System;
using System.Runtime.Serialization;
using Orchard.Localization;

namespace Orchard {
    public class OrchardSystemException : Exception {
        private readonly LocalizedString _localizedMessage;

        public OrchardSystemException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public OrchardSystemException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        protected OrchardSystemException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}