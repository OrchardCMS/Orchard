using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Orchard.AntiSpam.Models;
using Orchard.Logging;

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

        public async Task<SpamStatus> CheckForSpam(CommentCheckContext context) {
            try {
                var result = await ExecuteValidateRequest(context, "comment-check");

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

        public async Task ReportSpam(CommentCheckContext context) {
            try {
                var result = await ExecuteValidateRequest(context, "submit-spam");
            }
            catch (Exception e) {
                Logger.Error(e, "An error occured while reporting spam");
            }
        }

        public async Task ReportHam(CommentCheckContext context) {
            try {
                var result = await ExecuteValidateRequest(context, "submit-ham");
            }
            catch (Exception e) {
                Logger.Error(e, "An error occured while reporting ham");
            }
        }

        private async Task<string> ExecuteValidateRequest(CommentCheckContext context, string action) {
            var uri = String.Format(AkismetApiPattern, _apiKey, _endpoint, action);

            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";
            request.Timeout = 5000; //milliseconds
            request.ContentType = "application/x-www-form-urlencoded";

            var postData = "blog=" + HttpUtility.UrlEncode(context.Url)
                           + "&user_ip=" + HttpUtility.UrlEncode(context.UserIp)
                           + "&user_agent=" + HttpUtility.UrlEncode(context.UserAgent)
                           + "&referrer=" + HttpUtility.UrlEncode(context.Referrer)
                           + "&permalink=" + HttpUtility.UrlEncode(context.Permalink)
                           + "&comment_type=" + HttpUtility.UrlEncode(context.CommentType)
                           + "&comment_author=" + HttpUtility.UrlEncode(context.CommentAuthor)
                           + "&comment_author_email=" + HttpUtility.UrlEncode(context.CommentAuthorEmail)
                           + "&comment_author_url=" + HttpUtility.UrlEncode(context.CommentAuthorUrl)
                           + "&comment_content=" + HttpUtility.UrlEncode(context.CommentContent);

            byte[] content = Encoding.UTF8.GetBytes(postData);
            using (Stream stream = request.GetRequestStream()) {
                stream.Write(content, 0, content.Length);
            }
            using (WebResponse webResponse = await request.GetResponseAsync()) {
                using (var reader = new StreamReader(webResponse.GetResponseStream())) {
                    return await reader.ReadToEndAsync();
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
