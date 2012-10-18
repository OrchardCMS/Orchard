using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.AntiSpam.Services {
    public class AkismetApiSpamFilter : ISpamFilter {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly HttpContextBase _context;

        private const string AkismetApiPattern = "http://{0}.{1}/1.1/{2}";

        public AkismetApiSpamFilter(string endpoint, string apiKey, HttpContextBase context) {
            _endpoint = endpoint;
            _apiKey = apiKey;
            _context = context;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public SpamStatus CheckForSpam(string text) {
            try {
                var result = ExecuteValidateRequest(text, "comment-check");

                if (HandleValidateResponse(_context, result)) {
                    return SpamStatus.Spam;
                }

                return SpamStatus.Ham;
            }
            catch(Exception e) {
                Logger.Error(e, "An error occured while checking for spam");
                return SpamStatus.Spam;
            }
        }

        public void ReportSpam(string text) {
            try {
                var result = ExecuteValidateRequest(text, "submit-spam");
            }
            catch (Exception e) {
                Logger.Error(e, "An error occured while reporting spam");
            }
        }

        public void ReportHam(string text) {
            try {
                var result = ExecuteValidateRequest(text, "submit-ham");
            }
            catch (Exception e) {
                Logger.Error(e, "An error occured while reporting ham");
            }
        }

        private string ExecuteValidateRequest(string text, string action) {
            var uri = String.Format(AkismetApiPattern, _apiKey, _endpoint, action);

            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";
            request.Timeout = 5000; //milliseconds
            request.ContentType = "application/x-www-form-urlencoded";

            var postData = String.Format(CultureInfo.InvariantCulture, "blog={0}&user_ip={1}&user_agent={2}&referrer={3}&comment_content={4}",
                HttpUtility.UrlEncode(_context.Request.ToApplicationRootUrlString()),
                HttpUtility.UrlEncode(_context.Request.ServerVariables["REMOTE_ADDR"]),
                HttpUtility.UrlEncode(_context.Request.UserAgent),
                HttpUtility.UrlEncode(Convert.ToString(_context.Request.UrlReferrer)),
                HttpUtility.UrlEncode(text)
            );

            byte[] content = Encoding.UTF8.GetBytes(postData);
            using (Stream stream = request.GetRequestStream()) {
                stream.Write(content, 0, content.Length);
            }
            using (WebResponse webResponse = request.GetResponse()) {
                using (var reader = new StreamReader(webResponse.GetResponseStream())) {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static bool HandleValidateResponse(HttpContextBase context, string response) {
            if (!String.IsNullOrEmpty(response)) {
                string[] results = response.Split('\n');
                if (results.Length > 0) {
                    bool rval = Convert.ToBoolean(results[0], CultureInfo.InvariantCulture);
                    return rval;
                }
            }
            return false;
        }
    }
}
