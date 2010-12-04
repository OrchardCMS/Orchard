using Orchard.ContentManagement;
using System;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Email.Models {
    public class SmtpSettingsPart : ContentPart<SmtpSettingsPartRecord> {
        private readonly ComputedField<string> _password = new ComputedField<string>();

        public ComputedField<string> PasswordField {
            get { return _password; }
        }

        public string Address {
            get { return Record.Address; }
            set { Record.Address = value; }
        }

        public string Host {
            get { return Record.Host; }
            set { Record.Host = value; }
        }

        public int Port {
            get { return Record.Port; }
            set { Record.Port = value; }
        }

        public bool EnableSsl {
            get { return Record.EnableSsl; }
            set { Record.EnableSsl = value; }
        }

        public bool RequireCredentials {
            get { return Record.RequireCredentials; }
            set { Record.RequireCredentials = value; }
        }

        public string UserName {
            get { return Record.UserName; }
            set { Record.UserName = value; }
        }

        public string Password {
            get { return _password.Value; }
            set { _password.Value = value; }
        }

        public bool IsValid() {
            return !String.IsNullOrWhiteSpace(Record.Host)
                && Record.Port > 0
                && !String.IsNullOrWhiteSpace(Record.Address);
        }
    }
}