using System;
using System.Configuration;
using System.Net.Configuration;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Email.Services;

namespace Orchard.Email.Models {
    public class SmtpSettingsPart : ContentPart {
        private readonly ComputedField<string> _password = new ComputedField<string>();

        public ComputedField<string> PasswordField {
            get { return _password; }
        }

        public string Address {
            get { return this.Retrieve(x => x.Address); }
            set { this.Store(x => x.Address, value); }
        }

        private readonly LazyField<string> _addressPlaceholder = new LazyField<string>();
        internal LazyField<string> AddressPlaceholderField { get { return _addressPlaceholder; } }
        public string AddressPlaceholder { get { return _addressPlaceholder.Value; } }

        public string Host {
            get { return this.Retrieve(x => x.Host); }
            set { this.Store(x => x.Host, value); }
        }

        public int Port {
            get { return this.Retrieve(x => x.Port, 25); }
            set { this.Store(x => x.Port, value); }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        // EmailMessagingChannel is obsolete, but we need to mention it here.
        [Obsolete($"To keep configuration compatible with {nameof(EmailMessagingChannel)}.")]
#pragma warning restore CS0618 // Type or member is obsolete
        public bool EnableSsl => this.Retrieve(x => x.EnableSsl);

        public SmtpEncryptionMethod EncryptionMethod {
            get { return this.Retrieve(x => x.EncryptionMethod); }
            set { this.Store(x => x.EncryptionMethod, value); }
        }

        public bool AutoSelectEncryption {
            get { return this.Retrieve(x => x.AutoSelectEncryption, true); }
            set { this.Store(x => x.AutoSelectEncryption, value); }
        }

        public bool RequireCredentials {
            get { return this.Retrieve(x => x.RequireCredentials); }
            set { this.Store(x => x.RequireCredentials, value); }
        }

        public string UserName {
            get { return this.Retrieve(x => x.UserName); }
            set { this.Store(x => x.UserName, value); }
        }

        public string Password {
            get { return _password.Value; }
            set { _password.Value = value; }
        }

        public bool IsValid() {
            var section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            if (section != null && !String.IsNullOrWhiteSpace(section.Network.Host)) {
                return true;
            }

            if (String.IsNullOrWhiteSpace(Address)) {
                return false;
            }

            if (!String.IsNullOrWhiteSpace(Host) && Port == 0) {
                return false;
            }

            return true;
        }
    }
}