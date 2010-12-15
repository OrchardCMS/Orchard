using Orchard.ContentManagement.Records;

namespace Orchard.Email.Models {
    public class SmtpSettingsPartRecord : ContentPartRecord {
        /// <summary>
        /// From address in the mail message
        /// </summary>
        public virtual string Address { get; set; }
    
        /// <summary>
        /// Server name hosting the SMTP service
        /// </summary>
        public virtual string Host { get; set; }

        /// <summary>
        /// Port number on which SMTP service runs
        /// </summary>
        public virtual int Port { get; set; }

        /// <summary>
        /// Whether to enable SSL communications with the server
        /// </summary>
        public virtual bool EnableSsl { get; set; }

        /// <summary>
        /// Whether specific credentials should be used
        /// </summary>
        public virtual bool RequireCredentials { get; set; }

        /// <summary>
        /// The username to connect to the SMTP server if DefaultCredentials is False
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// The password to connect to the SMTP server if DefaultCredentials is False
        /// </summary>
        public virtual string Password { get; set; }

        public SmtpSettingsPartRecord() {
            Port = 25;
            RequireCredentials = false;
            EnableSsl = false;
        }
    }
}