using Orchard.Localization;

namespace Orchard.Users.Services
{

    public enum Severity
    {
        Warning,
        Fatal
    }

    public class UsernameValidationError
    {
        public UsernameValidationError(Severity severity, string key, LocalizedString errorMessage)
        {
            Severity = severity;
            Key = key;
            ErrorMessage = errorMessage;
        }

        public Severity Severity { get; set; }
        public string Key { get; set; }
        public LocalizedString ErrorMessage { get; set; }



    }
}