using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Email.Models;
using Orchard.Messaging.Services;
using Orchard.Messaging.Models;

namespace Orchard.Email.Services {
    public class EmailMessagingChannel : IMessagingChannel {
        private readonly IOrchardServices _orchardServices;

        public const string EmailService = "email";

        public EmailMessagingChannel(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void SendMessage(MessageContext context) {
            if ( !context.Service.Equals(EmailService, StringComparison.InvariantCultureIgnoreCase) )
                return;

            var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();

            // can't process emails if the Smtp settings have not yet been set
            if ( smtpSettings == null || !smtpSettings.IsValid() ) {
                return;
            }

            using (var smtpClient = new SmtpClient()) {
                smtpClient.UseDefaultCredentials = !smtpSettings.RequireCredentials;
                if (!smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(smtpSettings.UserName)) {
                    smtpClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
                }

                if (context.MailMessage.To.Count == 0) {
                    Logger.Error("Recipient is missing an email address");
                    return;
                }

                if (smtpSettings.Host != null)
                    smtpClient.Host = smtpSettings.Host;

                smtpClient.Port = smtpSettings.Port;
                smtpClient.EnableSsl = smtpSettings.EnableSsl;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                context.MailMessage.From = new MailAddress(smtpSettings.Address);
                context.MailMessage.IsBodyHtml = context.MailMessage.Body != null && context.MailMessage.Body.Contains("<") && context.MailMessage.Body.Contains(">");

                try {
                    smtpClient.Send(context.MailMessage);
                    Logger.Debug("Message sent to {0}: {1}", context.MailMessage.To[0].Address, context.Type);
                }
                catch (Exception e) {
                    Logger.Error(e, "An unexpected error while sending a message to {0}: {1}", context.MailMessage.To[0].Address, context.Type);
                }
            }
        }

        public IEnumerable<string> GetAvailableServices() {
            return new[] {EmailService};
        }
    }
}
