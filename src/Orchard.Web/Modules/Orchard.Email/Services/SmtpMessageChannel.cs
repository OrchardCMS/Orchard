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
                From = Read(parameters, "From"),
                Bcc = Read(parameters, "Bcc"),
                Cc = Read(parameters, "CC")
            };

            if (emailMessage.Recipients.Length == 0) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            // Apply default Body alteration for SmtpChannel.
            var template = _shapeFactory.Create("Template_Smtp_Wrapper", Arguments.From(new {
                Content = new MvcHtmlString(emailMessage.Body)
            }));

            var mailMessage = new MailMessage {
                Subject = emailMessage.Subject,
                Body = _shapeDisplay.Display(template),
                IsBodyHtml = true
            };

            if (parameters.ContainsKey("Message")) {
                // A full message object is provided by the sender.

                var oldMessage = mailMessage;
                mailMessage = (MailMessage)parameters["Message"];

                if (String.IsNullOrWhiteSpace(mailMessage.Subject))
                    mailMessage.Subject = oldMessage.Subject;

                if (String.IsNullOrWhiteSpace(mailMessage.Body)) {
                    mailMessage.Body = oldMessage.Body;
                    mailMessage.IsBodyHtml = oldMessage.IsBodyHtml;
                }
            }

            try {

                foreach (var recipient in ParseRecipients(emailMessage.Recipients)) {
                    mailMessage.To.Add(new MailAddress(recipient));
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.Cc)) {
                    foreach (var recipient in ParseRecipients(emailMessage.Cc)) {
                        mailMessage.CC.Add(new MailAddress(recipient));
                    }
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.Bcc)) {
                    foreach (var recipient in ParseRecipients(emailMessage.Bcc)) {
                        mailMessage.Bcc.Add(new MailAddress(recipient));
                    }
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.From)) {
                    mailMessage.From = new MailAddress(emailMessage.From);
                }
                else {
                    // Take 'From' address from site settings or web.config.
                    mailMessage.From = !String.IsNullOrWhiteSpace(_smtpSettings.Address)
                        ? new MailAddress(_smtpSettings.Address)
                        : new MailAddress(((SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp")).From);
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.ReplyTo)) {
                    foreach (var recipient in ParseRecipients(emailMessage.ReplyTo)) {
                        mailMessage.ReplyToList.Add(new MailAddress(recipient));
                    }
                }

                _smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private SmtpClient CreateSmtpClient() {
            // If no properties are set in the dashboard, use the web.config value.
            if (String.IsNullOrWhiteSpace(_smtpSettings.Host)) {
                return new SmtpClient(); 
            }

            var smtpClient = new SmtpClient {
                UseDefaultCredentials = !_smtpSettings.RequireCredentials,
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

        private string Read(IDictionary<string, object> dictionary, string key) {
            return dictionary.ContainsKey(key) ? dictionary[key] as string : null;
        }

        private IEnumerable<string> ParseRecipients(string recipients) {
            return recipients.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}