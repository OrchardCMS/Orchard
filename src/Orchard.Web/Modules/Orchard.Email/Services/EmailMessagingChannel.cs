using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Core.Messaging.Models;
using Orchard.Core.Messaging.Services;
using Orchard.Email.Models;
using Orchard.Settings;
using Orchard.Messaging.Services;
using Orchard.Messaging.Models;

namespace Orchard.Email.Services {
    public class EmailMessagingChannel : IMessagingChannel {

        public const string EmailService = "Email";
        public const string EmailAddress = "EmailAddress";

        public EmailMessagingChannel() {
            Logger = NullLogger.Instance;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void SendMessage(MessageContext context) {
            if ( context.Message.Service.ToLower() != EmailService )
                return;

            var smtpSettings = CurrentSite.As<SmtpSettingsPart>();

            // can't process emails if the Smtp settings have not yet been set
            if ( smtpSettings == null || !smtpSettings.IsValid() ) {
                return;
            }

            var smtpClient = new SmtpClient { UseDefaultCredentials = !smtpSettings.RequireCredentials };
            if ( !smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(smtpSettings.UserName) ) {
                smtpClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
            }

            var emailAddress = context.Properties[EmailAddress];

            if(String.IsNullOrWhiteSpace(emailAddress)) {
                Logger.Error("Recipient is missing an email address");
                return;
            }

            if ( smtpSettings.Host != null )
                smtpClient.Host = smtpSettings.Host;

            smtpClient.Port = smtpSettings.Port;
            smtpClient.EnableSsl = smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            var message = new MailMessage {
                From = new MailAddress(smtpSettings.Address), 
                Subject = context.Message.Subject ?? "", 
                Body = context.Message.Body ?? "",
                IsBodyHtml = context.Message.Body != null && context.Message.Body.Contains("<") && context.Message.Body.Contains(">")
            };

            message.To.Add(emailAddress);

            try {
                smtpClient.Send(message);
                Logger.Debug("Message sent to {0}: {1}", emailAddress, context.Message.Subject);
            }
            catch(Exception e) {
                Logger.Error(e, "An unexpected error while sending a message to {0}: {1}", emailAddress, context.Message.Subject);
            }
        }

        public bool IsRecipientValidated(ContentItem contentItem) {
            return false;
        }

        public void ValidateRecipient(ContentItem contentItem) {
            var context = new MessageContext(new Message { Recipient = contentItem.Record, Body = "Please validate your account", Service = "email", Subject = "Validate your account" } );
            SendMessage(context);
        }

        public IEnumerable<string> GetAvailableServices() {
            return new[] {EmailService};
        }
    }
}
