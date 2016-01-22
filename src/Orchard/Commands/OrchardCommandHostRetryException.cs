using System;
using System.Runtime.Serialization;
using Orchard.Localization;

namespace Orchard.Commands {
    [Serializable]
    public class OrchardCommandHostRetryException : OrchardCoreException {
        public OrchardCommandHostRetryException(LocalizedString message)
            : base(message) {
        }

        public OrchardCommandHostRetryException(LocalizedString message, Exception innerException)
            : base(message, innerException) {
        }

        protected OrchardCommandHostRetryException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}