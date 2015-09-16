using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;

namespace Orchard.Email.Controllers {
    [Admin]
    public class EmailAdminController : Controller {
        private readonly ISmtpChannel _smtpChannel;
        private readonly IOrchardServices _orchardServices;

        public EmailAdminController(ISmtpChannel smtpChannel, IOrchardServices orchardServices) {
            _smtpChannel = smtpChannel;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TestSettings(TestSmtpSettings testSettings) {
            ILogger logger = null;
            try {
                var fakeLogger = new FakeLogger();
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null) {
                    logger = smtpChannelComponent.Logger;
                    smtpChannelComponent.Logger = fakeLogger;
                }

                // Temporarily update settings so that the test will actually use the specified host, port, etc.
                var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();

                smtpSettings.Address = testSettings.From;
                smtpSettings.Host = testSettings.Host;
                smtpSettings.Port = testSettings.Port;
                smtpSettings.EnableSsl = testSettings.EnableSsl;
                smtpSettings.RequireCredentials = testSettings.RequireCredentials;
                smtpSettings.UserName = testSettings.UserName;
                smtpSettings.Password = testSettings.Password;

                if (!smtpSettings.IsValid()) {
                    fakeLogger.Error("Invalid settings.");
                }
                else {
                    _smtpChannel.Process(new Dictionary<string, object> {
                        {"Recipients", testSettings.To},
                        {"Subject", testSettings.Subject},
                        {"Body", testSettings.Body},
                        {"ReplyTo", testSettings.ReplyTo},
                        {"Bcc", testSettings.Bcc},
                        {"CC", testSettings.Cc}
                    });
                }

                if (!String.IsNullOrEmpty(fakeLogger.Message)) {
                    return Json(new { error = fakeLogger.Message });
                }

                return Json(new {status = T("Message sent.").Text});
            }
            catch (Exception e) {
                return Json(new {error = e.Message});
            }
            finally {
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null) {
                    smtpChannelComponent.Logger = logger;
                }

                // Undo the temporarily changed smtp settings.
                _orchardServices.TransactionManager.Cancel();
            }
        }

        private class FakeLogger : ILogger {
            public string Message { get; set; }

            public bool IsEnabled(LogLevel level) {
                return true;
            }

            public void Log(LogLevel level, Exception exception, string format, params object[] args) {
                Message = exception == null ? format : exception.Message;
            }
        }

        public class TestSmtpSettings {
            public string From { get; set; }
            public string ReplyTo { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
            public bool RequireCredentials { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string To { get; set; }
            public string Cc { get; set; }
            public string Bcc { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}