using System;
using System.Configuration;
using System.Net.Configuration;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

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

        [Obsolete($"Use {nameof(AutoSelectEncryption)} and/or {nameof(EncryptionMethod)} instead.")]
        public bool EnableSsl => this.Retrieve(x => x.EnableSsl);

        public SmtpEncryptionMethod EncryptionMethod {
#pragma warning disable CS0618 // Type or member is obsolete
            // Reading EnableSsl is necessary to keep the correct settings during the upgrade to MailKit.
            get { return this.Retrieve(x => x.EncryptionMethod, EnableSsl ? SmtpEncryptionMethod.SslTls : SmtpEncryptionMethod.None); }
#pragma warning restore CS0618 // Type or member is obsolete
            set { this.Store(x => x.EncryptionMethod, value); }
        }

        public bool AutoSelectEncryption {
#pragma warning disable CS0618 // Type or member is obsolete
            // Reading EnableSsl is necessary to keep the correct settings during the upgrade to MailKit.
            get { return this.Retrieve(x => x.AutoSelectEncryption, !EnableSsl); }
#pragma warning restore CS0618 // Type or member is obsolete
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