using System;
using System.Runtime.Serialization;
using Orchard.Localization;

namespace Orchard {
    [Serializable]
    public class OrchardFatalException : Exception {
        private readonly LocalizedString _localizedMessage;

        public OrchardFatalException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public OrchardFatalException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        protected OrchardFatalException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}
