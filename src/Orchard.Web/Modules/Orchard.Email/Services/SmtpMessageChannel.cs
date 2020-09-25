using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Email.Models;
using System.IO;

namespace Orchard.Email.Services {
    public class SmtpMessageChannel : Component, ISmtpChannel, IDisposable {
        private readonly SmtpSettingsPart smtpSettings;
        private readonly IShapeFactory shapeFactory;
        private readonly IShapeDisplay shapeDisplay;
        private readonly Lazy<SmtpClient> smtpClientField;
        public static readonly string MessageType = "Email";

        public SmtpMessageChannel(
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay) {

            this.shapeFactory = shapeFactory;
            this.shapeDisplay = shapeDisplay;

            smtpSettings = orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
        }

        public void Dispose() {
            if (!smtpClientField.IsValueCreated) {
                return;
            }

            smtpClientField.Value.Dispose();
        }

        public void Process(IDictionary<string, object> parameters) {
            if (!smtpSettings.IsValid()) {
                return;
            }

            var emailMessage = new EmailMessage {
                Body = Read(parameters, "Body"),
                Subject = Read(parameters, "Subject"),
                Recipients = Read(parameters, "Recipients"),
                ReplyTo = Read(parameters, "ReplyTo"),
                FromAddress = Read(parameters, "FromAddress"),
                FromName = Read(parameters, "FromName"),
                Bcc = Read(parameters, "Bcc"),
                Cc = Read(parameters, "CC"),
                Attachments = (IEnumerable<string>)(
                    parameters.ContainsKey("Attachments")
                        ? parameters["Attachments"]
                        : new List<string>()
                )
            };

            if (string.IsNullOrWhiteSpace(emailMessage.Recipients)) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            var mailMessage = CreteMailMessage(parameters, emailMessage);

            try {
                smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private MailMessage CreteMailMessage(IDictionary<string, object> parameters, EmailMessage emailMessage) {

            // Apply default Body alteration for SmtpChannel.
            var template = shapeFactory.Create("Template_Smtp_Wrapper", Arguments.From(new {
                Content = new MvcHtmlString(emailMessage.Body)
            }));

            var mailMessage = new MailMessage {
                Subject = emailMessage.Subject,
                Body = shapeDisplay.Display(template),
                IsBodyHtml = true
            };

            if (parameters.ContainsKey("Message")) {
                // A full message object is provided by the sender.
                var oldMessage = mailMessage;
                mailMessage = (MailMessage)parameters["Message"];

                if (string.IsNullOrWhiteSpace(mailMessage.Subject))
                    mailMessage.Subject = oldMessage.Subject;

                if (string.IsNullOrWhiteSpace(mailMessage.Body)) {
                    mailMessage.Body = oldMessage.Body;
                    mailMessage.IsBodyHtml = oldMessage.IsBodyHtml;
                }
            }

            foreach (var recipient in ParseRecipients(emailMessage.Recipients)) {
                mailMessage.To.Add(new MailAddress(recipient));
            }

            if (!string.IsNullOrWhiteSpace(emailMessage.Cc)) {
                foreach (var recipient in ParseRecipients(emailMessage.Cc)) {
                    mailMessage.CC.Add(new MailAddress(recipient));
                }
            }

            if (!string.IsNullOrWhiteSpace(emailMessage.Bcc)) {
                foreach (var recipient in ParseRecipients(emailMessage.Bcc)) {
                    mailMessage.Bcc.Add(new MailAddress(recipient));
                }
            }

            var senderAddress =
                !string.IsNullOrWhiteSpace(emailMessage.FromAddress) ? emailMessage.FromAddress :
                !string.IsNullOrWhiteSpace(smtpSettings.FromAddress) ? smtpSettings.FromAddress :
                // Take 'From' address from site settings or web.config.
                ((SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp")).From;

            var senderName = !string.IsNullOrWhiteSpace(emailMessage.FromName)
                ? emailMessage.FromName
                : smtpSettings.FromName;

            var sender = (senderAddress, senderName) switch
            {
                (string address, string name) => new MailAddress(address, name),
                (string address, null) => new MailAddress(address),
                _ => throw new InvalidOperationException("No sender email address")
            };
            mailMessage.From = sender;

            var replyTo =
                !string.IsNullOrWhiteSpace(emailMessage.ReplyTo) ? ParseRecipients(emailMessage.ReplyTo) :
                !string.IsNullOrWhiteSpace(smtpSettings.ReplyTo) ? new[] { smtpSettings.ReplyTo } :
                Array.Empty<string>();

            foreach (var recipient in replyTo) {
                mailMessage.ReplyToList.Add(new MailAddress(recipient));
            }

            foreach (var attachmentPath in emailMessage.Attachments) {
                if (File.Exists(attachmentPath)) {
                    mailMessage.Attachments.Add(new Attachment(attachmentPath));
                }
                else {
                    throw new FileNotFoundException(T("One or more attachments not found.").Text);
                }
            }

            if (parameters.ContainsKey("NotifyReadEmail")) {
                if (parameters["NotifyReadEmail"] is bool) {
                    if ((bool)(parameters["NotifyReadEmail"])) {
                        mailMessage.Headers.Add("Disposition-Notification-To", mailMessage.From.ToString());
                    }
                }
            }

            return mailMessage;
        }

        private SmtpClient CreateSmtpClient() {
            // If no properties are set in the dashboard, use the web.config value.
            if (string.IsNullOrWhiteSpace(smtpSettings.Host)) {
                return new SmtpClient();
            }

            var smtpClient = new SmtpClient {
                UseDefaultCredentials = smtpSettings.RequireCredentials && smtpSettings.UseDefaultCredentials
            };

            if (!smtpClient.UseDefaultCredentials && !string.IsNullOrWhiteSpace(smtpSettings.UserName)) {
                smtpClient.Credentials = new NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
            }

            if (smtpSettings.Host != null) {
                smtpClient.Host = smtpSettings.Host;
            }

            smtpClient.Port = smtpSettings.Port;
            smtpClient.EnableSsl = smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }

        private string Read(IDictionary<string, object> dictionary, string key) =>
            dictionary.ContainsKey(key) ? dictionary[key] as string : null;

        private IEnumerable<string> ParseRecipients(string recipients) =>
            recipients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
