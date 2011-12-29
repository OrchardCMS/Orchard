using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Environment.Extensions {
    public class DefaultCriticalErrorProvider : ICriticalErrorProvider {
        private readonly ConcurrentBag<LocalizedString> _errorMessages;   

        public DefaultCriticalErrorProvider() {
            _errorMessages = new ConcurrentBag<LocalizedString>();

        }

        public IEnumerable<LocalizedString> GetErrors() {
            return _errorMessages;
        }

        public void RegisterErrorMessage(LocalizedString message) {
            if (_errorMessages != null && _errorMessages.All(m => m.TextHint != message.TextHint)) {
                _errorMessages.Add(message);
            }
        }

    }
}
