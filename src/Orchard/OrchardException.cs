using System;
using System.Runtime.Serialization;
using Orchard.Localization;

namespace Orchard {
    [Serializable]
    public class OrchardException : ApplicationException {
        private readonly LocalizedString _localizedMessage;

        public OrchardException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public OrchardException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        protected OrchardException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}