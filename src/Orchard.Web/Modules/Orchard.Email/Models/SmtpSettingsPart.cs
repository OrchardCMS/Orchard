using System.Configuration;
using System.Net.Configuration;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Email.Models {
    public class SmtpSettingsPart : ContentPart {
        private readonly ComputedField<string> password = new ComputedField<string>();
        private readonly LazyField<string> addressPlaceholder = new LazyField<string>();
        internal LazyField<string> AddressPlaceholderField => addressPlaceholder;

        public ComputedField<string> PasswordField => password;

        public string FromAddress {
            get => this.Retrieve(x => x.FromAddress);
            set => this.Store(x => x.FromAddress, value);
        }

        public string FromName {
            get => this.Retrieve(x => x.FromName);
            set => this.Store(x => x.FromName, value);
        }

        public string ReplyTo {
            get => this.Retrieve(x => x.ReplyTo);
            set => this.Store(x => x.ReplyTo, value);
        }

        public string AddressPlaceholder => addressPlaceholder.Value;

        public string Host {
            get => this.Retrieve(x => x.Host);
            set => this.Store(x => x.Host, value);
        }

        public int Port {
            get => this.Retrieve(x => x.Port, 25);
            set => this.Store(x => x.Port, value);
        }

        public bool EnableSsl {
            get => this.Retrieve(x => x.EnableSsl);
            set => this.Store(x => x.EnableSsl, value);
        }

        public bool RequireCredentials {
            get => this.Retrieve(x => x.RequireCredentials);
            set => this.Store(x => x.RequireCredentials, value);
        }

        public bool UseDefaultCredentials {
            get => this.Retrieve(x => x.UseDefaultCredentials);
            set => this.Store(x => x.UseDefaultCredentials, value);
        }

        public string UserName {
            get => this.Retrieve(x => x.UserName);
            set => this.Store(x => x.UserName, value);
        }

        public string Password {
            get => password.Value;
            set => password.Value = value;
        }

        public bool IsValid() {
            var section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            if (section != null && !string.IsNullOrWhiteSpace(section.Network.Host)) {
                return true;
            }

            if (string.IsNullOrWhiteSpace(FromAddress)) {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Host) && Port == 0) {
                return false;
            }

            return true;
        }
    }
}