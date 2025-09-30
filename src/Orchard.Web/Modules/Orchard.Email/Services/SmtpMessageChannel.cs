using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Mvc;
using MailKit.Security;
using MimeKit;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Email.Models;
using Orchard.Logging;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Orchard.Email.Services {
    public class SmtpMessageChannel : Component, ISmtpChannel, IDisposable {
        private readonly SmtpSettingsPart _smtpSettings;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly Lazy<SmtpClient> _smtpClientField;
        public static readonly string MessageType = "Email";

        public SmtpMessageChannel(
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay) {

            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            _smtpSettings = orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            _smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
        }

        public void Dispose() {
            if (!_smtpClientField.IsValueCreated) {
                return;
            }

            _smtpClientField.Value.Dispose();
        }

        public void Process(IDictionary<string, object> parameters) {

            if (!_smtpSettings.IsValid()) {
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
                NotifyReadEmail = bool.TryParse(Read(parameters, "NotifyReadEmail"), out var notify) && notify,
                Attachments = (IEnumerable<string>)(parameters.ContainsKey("Attachments")
                    ? parameters["Attachments"]
                    : new List<string>()
                )
            };

            if (string.IsNullOrWhiteSpace(emailMessage.Recipients)) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            // Apply default Body alteration for SmtpChannel.
            var template = _shapeFactory.Create("Template_Smtp_Wrapper", Arguments.From(new {
                Content = new MvcHtmlString(emailMessage.Body)
            }));

            var mailMessage = new MimeMessage {
                Subject = emailMessage.Subject,
            };
            var mailBodyBuilder = new BodyBuilder {
                HtmlBody = _shapeDisplay.Display(template),
            };

            if (parameters.TryGetValue("Message", out var possiblyMailMessage) && possiblyMailMessage is MailMessage legacyMessage) {
                // A full message object is provided by the sender.
                if (!String.IsNullOrWhiteSpace(legacyMessage.Subject)) {
                    mailMessage.Subject = legacyMessage.Subject;
                }

                if (!String.IsNullOrWhiteSpace(legacyMessage.Body)) {
                    mailBodyBuilder.TextBody = legacyMessage.IsBodyHtml ? null : legacyMessage.Body;
                    mailBodyBuilder.HtmlBody = legacyMessage.IsBodyHtml ? legacyMessage.Body : null;
                }
            }

            foreach (var attachmentPath in emailMessage.Attachments) {
                if (File.Exists(attachmentPath)) {
                    mailBodyBuilder.Attachments.Add(attachmentPath);
                }
                else {
                    throw new FileNotFoundException(T("One or more attachments not found.").Text);
                }
            }

            mailMessage.Body = mailBodyBuilder.ToMessageBody();

            try {
                mailMessage.To.AddRange(ParseRecipients(emailMessage.Recipients));

                mailMessage.Cc.AddRange(ParseRecipients(emailMessage.Cc));

                mailMessage.Bcc.AddRange(ParseRecipients(emailMessage.Bcc));

                var fromAddress = MailboxAddress.Parse(
                    // "From" address precendence: Current email message > site settings > configuration.
                    string.IsNullOrWhiteSpace(emailMessage.FromAddress)
                        ? string.IsNullOrWhiteSpace(_smtpSettings.FromAddress)
                            ? ((SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp")).From
                            : _smtpSettings.FromAddress
                        : emailMessage.FromAddress);
                fromAddress.Name = string.IsNullOrWhiteSpace(emailMessage.FromName) ? _smtpSettings.FromName : emailMessage.FromName;
                mailMessage.From.Add(fromAddress);

                mailMessage.ReplyTo.AddRange(ParseRecipients(string.IsNullOrWhiteSpace(emailMessage.ReplyTo)
                    ? _smtpSettings.ReplyTo
                    : emailMessage.ReplyTo));

                if (emailMessage.NotifyReadEmail) {
                    mailMessage.Headers.Add("Disposition-Notification-To", mailMessage.From.ToString());
                }

                _smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private SmtpClient CreateSmtpClient() {
            var smtpConfiguration = new {
                _smtpSettings.Host,
                _smtpSettings.Port,
                _smtpSettings.EncryptionMethod,
                _smtpSettings.AutoSelectEncryption,
                _smtpSettings.RequireCredentials,
                _smtpSettings.UserName,
                _smtpSettings.Password,
            };
            // If no properties are set in the dashboard, use the web.config value.
            if (String.IsNullOrWhiteSpace(_smtpSettings.Host)) {
                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                if (smtpSection.DeliveryMethod != SmtpDeliveryMethod.Network) {
                    throw new NotSupportedException($"Only the {SmtpDeliveryMethod.Network} delivery method is supported, but "
                        + $"{smtpSection.DeliveryMethod} delivery method is configured. Please check your Web.config.");
                }

                smtpConfiguration = new {
                    smtpSection.Network.Host,
                    smtpSection.Network.Port,
                    EncryptionMethod = smtpSection.Network.EnableSsl ? SmtpEncryptionMethod.SslTls : SmtpEncryptionMethod.None,
                    AutoSelectEncryption = !smtpSection.Network.EnableSsl,
                    RequireCredentials = smtpSection.Network.DefaultCredentials || !String.IsNullOrWhiteSpace(smtpSection.Network.UserName),
                    smtpSection.Network.UserName,
                    smtpSection.Network.Password,
                };
            }

            var secureSocketOptions = SecureSocketOptions.Auto;
            if (!smtpConfiguration.AutoSelectEncryption) {
                switch (smtpConfiguration.EncryptionMethod) {
                    case SmtpEncryptionMethod.SslTls:
                        secureSocketOptions = SecureSocketOptions.SslOnConnect;
                        break;
                    case SmtpEncryptionMethod.StartTls:
                        secureSocketOptions = SecureSocketOptions.StartTls;
                        break;
                    default:
                        secureSocketOptions = SecureSocketOptions.None;
                        break;
                }
            }

            var smtpClient = new SmtpClient();
            smtpClient.Connect(smtpConfiguration.Host, smtpConfiguration.Port, secureSocketOptions);

            if (smtpConfiguration.RequireCredentials) {
                smtpClient.Authenticate(smtpConfiguration.UserName, smtpConfiguration.Password);
            }

            return smtpClient;
        }

        private string Read(IDictionary<string, object> dictionary, string key) {
            return dictionary.ContainsKey(key) ? dictionary[key] as string : null;
        }

        private IEnumerable<MailboxAddress> ParseRecipients(string recipients) {
            return recipients?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(address => {
                    if (MailboxAddress.TryParse(address, out var mailboxAddress)) {
                        return new[] { mailboxAddress };
                    }

                    Logger.Error("Invalid email address: {0}", address);
                    return Enumerable.Empty<MailboxAddress>();
                }) ?? Enumerable.Empty<MailboxAddress>();
        }
    }
}
