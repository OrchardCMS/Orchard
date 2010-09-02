using System.Text;
using System.Web.Security;
using Orchard.ContentManagement;
using System;

namespace Orchard.Email.Models {
    public class SmtpSettingsPart : ContentPart<SmtpSettingsPartRecord> {
        public bool IsValid() {
            return !String.IsNullOrWhiteSpace(Record.Host)
                && Record.Port > 0
                && !String.IsNullOrWhiteSpace(Record.Address);
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
            get { return String.IsNullOrWhiteSpace(Record.Password) ? String.Empty : Encoding.UTF8.GetString(MachineKey.Decode(Record.Password, MachineKeyProtection.All)); ; }
            set { Record.Password = String.IsNullOrWhiteSpace(value) ? String.Empty : MachineKey.Encode(Encoding.UTF8.GetBytes(value), MachineKeyProtection.All); }
        }
    }
}