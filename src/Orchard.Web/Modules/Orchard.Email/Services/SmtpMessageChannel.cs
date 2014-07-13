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
                Body = parameters["Body"] as string,
                Subject = parameters["Subject"] as string,
                Recipients = parameters["Recipients"] as string
            };

            if (emailMessage.Recipients.Length == 0) {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            // Applying default Body alteration for SmtpChannel
            var template = _shapeFactory.Create("Template_Smtp_Wrapper", Arguments.From(new {
                Content = new MvcHtmlString(emailMessage.Body)
            }));

            var mailMessage = new MailMessage {
                Subject = emailMessage.Subject,
                Body = _shapeDisplay.Display(template),
                IsBodyHtml = true
            };

            var section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            mailMessage.From = !String.IsNullOrWhiteSpace(_smtpSettings.Address) 
                ? new MailAddress(_smtpSettings.Address) 
                : new MailAddress(section.From);

            try {
                foreach (var recipient in emailMessage.Recipients.Split(new [] {',', ';'}, StringSplitOptions.RemoveEmptyEntries)) {
                    mailMessage.To.Add(new MailAddress(recipient));
                }

                _smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e) {
                Logger.Error(e, "Could not send email");
            }
        }

        private SmtpClient CreateSmtpClient() {
            // if no properties are set in the dashboard, use the web.config value
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
    }
}
