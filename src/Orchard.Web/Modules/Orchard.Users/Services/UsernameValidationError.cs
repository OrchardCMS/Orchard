using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;

namespace Orchard.Users.Services {

    public enum Severity {
        Warning,
        Fatal
    }

    public class UsernameValidationError {

        private Severity _severity;
        private string _key;
        private LocalizedString _errorMessage;

        public UsernameValidationError(Severity severity, string key, LocalizedString errorMessage) {
            Severity = severity;
            Key = key;
            ErrorMessage = errorMessage;
        }

        public Severity Severity { get => _severity; set => _severity = value; }
        public string Key { get => _key; set => _key = value; }
        public LocalizedString ErrorMessage { get => _errorMessage; set => _errorMessage = value; }



    }
}