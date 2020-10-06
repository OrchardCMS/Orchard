using System.Configuration;
using System.Net.Configuration;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Email.Models {
    public class SmtpSettingsPart : ContentPart {
        private readonly ComputedField<string> _password = new ComputedField<string>();

        public ComputedField<string> PasswordField => _password;

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

        private readonly LazyField<string> _addressPlaceholder = new LazyField<string>();
        internal LazyField<string> AddressPlaceholderField => _addressPlaceholder;
        public string AddressPlaceholder => _addressPlaceholder.Value;

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
            get => _password.Value;
            set => _password.Value = value;
        }

        // Hotmail only supports the mailto:link. When a user clicks on the 'unsubscribe' option in Hotmail. 
        // Hotmail tries to read the mailto:link in the List-Unsubscribe header. 
        // If the mailto:link is missing, it moves all the messages to the Junk folder.
        // The mailto:link is supported by Gmail, Hotmail, Yahoo, AOL, ATT, Time Warner and Comcast; 
        // European ISPs such as GMX, Libero, Ziggo, Orange, BTInternet; Russian ISPs such as mail.ru and Yandex; 
        // and the Chinese domains qq.com, naver.com etc. So most ISPs support (and prefer) mailto:link.
        public string ListUnsubscribe {
            get => this.Retrieve(x => x.ListUnsubscribe);
            set => this.Store(x => x.ListUnsubscribe, value);
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