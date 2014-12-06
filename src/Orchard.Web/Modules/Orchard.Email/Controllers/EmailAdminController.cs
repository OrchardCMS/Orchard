using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;

namespace Orchard.Email.Controllers {
    [Admin]
    public class EmailAdminController : Controller {
        private readonly ISmtpChannel _smtpChannel;

        public EmailAdminController(ISmtpChannel smtpChannel) {
            _smtpChannel = smtpChannel;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult TestMailSettings(string to, string subject, string body, string replyTo, string bcc, string cc) {
            ILogger logger = null;
            try {
                var fakeLogger = new FakeLogger();
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null) {
                    logger = smtpChannelComponent.Logger;
                    smtpChannelComponent.Logger = fakeLogger;
                }
                _smtpChannel.Process(new Dictionary<string, object> {
                    {"Recipients", to},
                    {"Subject", subject},
                    {"Body", body},
                    {"ReplyTo",replyTo},
                    {"Bcc", bcc},
                    {"CC",cc}
                });
                if (!string.IsNullOrEmpty(fakeLogger.Message)) {
                    return Json(new { error = fakeLogger.Message });
                }
                return Json(new {status = T("Message sent").Text});
            }
            catch (Exception e) {
                return Json(new {error = e.Message});
            }
            finally {
                var smtpChannelComponent = _smtpChannel as Component;
                if (smtpChannelComponent != null) {
                    smtpChannelComponent.Logger = logger;
                }
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
    }
}