using System;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;
using Orchard.Logging;
using Orchard.Email.Models;
using Orchard.Messaging.Services;

namespace Orchard.Email.Services {
    public class SmtpMessageChannel : Component, IMessageChannel, IDisposable {
        private readonly SmtpSettingsPart _smtpSettings;
        private readonly Lazy<SmtpClient> _smtpClientField;
        public static readonly string MessageType = "Email";

        public SmtpMessageChannel(SmtpSettingsPart smtpSettings) {
            _smtpSettings = smtpSettings;
            _smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
        }

        public void Dispose() {
            if (!_smtpClientField.IsValueCreated) {
                return;
            }

            _smtpClientField.Value.Dispose();
        }

        public void Process(string payload) {
            if (!_smtpSettings.IsValid()) {
                return;
            }

            var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(payload);
            if (emailMessage == null) {
                return;
            }

            if (emailMessage.Recipients.Length == 0) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            var mailMessage = new MailMessage {
                From = new MailAddress(_smtpSettings.Address),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = emailMessage.Body != null && emailMessage.Body.Contains("<") && emailMessage.Body.Contains(">")
            };

            try {
                foreach (var recipient in emailMessage.Recipients) {
                    mailMessage.To.Add(new MailAddress(recipient));
                }

                _smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private SmtpClient CreateSmtpClient() {
            var smtpClient = new SmtpClient {
                UseDefaultCredentials = !_smtpSettings.RequireCredentials
            };

            if (!smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(_smtpSettings.UserName)) {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
            }

            if (_smtpSettings.Host != null) {
                smtpClient.Host = _smtpSettings.Host;
            }

            smtpClient.Port = _smtpSettings.Port;
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }
    }
}
