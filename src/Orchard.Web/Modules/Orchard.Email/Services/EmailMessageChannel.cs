using System;
using System.Net;
using System.Net.Mail;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Email.Models;
using Orchard.Messaging.Services;
using Orchard.Messaging.Models;

namespace Orchard.Email.Services {
    public interface IEmailMessageChannel : IMessageChannel { }

    public class EmailMessageChannel : MessageChannelBase, IEmailMessageChannel {
        private readonly IOrchardServices _services;
        private readonly Lazy<SmtpClient> _smtpClientField;
        public const string ChannelName = "Email";

        public EmailMessageChannel(IOrchardServices services) {
            _services = services;
            _smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
            Logger = NullLogger.Instance;
        }

        public override string Name {
            get { return ChannelName; }
        }

        private SmtpClient SmtpClient {
            get { return _smtpClientField.Value; }
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) return;
            if (!_smtpClientField.IsValueCreated) return;

            _smtpClientField.Value.Dispose();
        }

        public override void Send(QueuedMessage message) {
            var smtpSettings = _services.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            if (smtpSettings == null || !smtpSettings.IsValid()) return;

            var emailPayload = message.GetPayload<EmailMessage>();
            var mailMessage = new MailMessage {
                From = new MailAddress(smtpSettings.Address),
                Subject = emailPayload.Subject,
                Body = emailPayload.Body,
                IsBodyHtml = emailPayload.Body != null && emailPayload.Body.Contains("<") && emailPayload.Body.Contains(">")
            };

            foreach (var recipient in message.Recipients) {
                mailMessage.To.Add(new MailAddress(recipient.AddressOrAlias));
            }

            SmtpClient.Send(mailMessage);
        }

        private SmtpClient CreateSmtpClient() {
            var smtpSettings = _services.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            var smtpClient = new SmtpClient {
                UseDefaultCredentials = !smtpSettings.RequireCredentials
            };
           
            if (!smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(smtpSettings.UserName)) {
                smtpClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
            }

            if (smtpSettings.Host != null)
                smtpClient.Host = smtpSettings.Host;

            smtpClient.Port = smtpSettings.Port;
            smtpClient.EnableSsl = smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }
    }
}
