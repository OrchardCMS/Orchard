using System.IO;
using System.Net;
using TechTalk.SpecFlow;

namespace Orchard.Profile.Tests {
    [Binding]
    public class HttpClient {
        private readonly CookieContainer _cookieContainer = new CookieContainer();
        private HttpWebRequest _request;
        private HttpWebResponse _response;
        private string _text;

        [Given(@"I am logged in")]
        public void GivenIAmLoggedIn() {
            DoRequest("/Users/Account/LogOn");

            const string requestVerificationTokenName = "__RequestVerificationToken";
            const string valueMarker = "value=\"";

            var tokenIndex = _text.IndexOf(requestVerificationTokenName);
            var valueIndex = _text.IndexOf(valueMarker, tokenIndex);
            var valueStart = valueIndex + valueMarker.Length;
            var valueEnd = _text.IndexOf("\"", valueStart);
            var requestVerificationTokenValue = _text.Substring(valueStart, valueEnd - valueStart);

            DoRequest("/Users/Account/LogOn", "userNameOrEmail=admin&password=profiling-secret&rememberMe=false&" + requestVerificationTokenName + "=" + requestVerificationTokenValue);
        }

        [When(@"I go to ""(.*)""")]
        public void WhenIGoTo(string url) {
            DoRequest(url);
        }

        [When(@"I go to ""(.*)"" (.*) times")]
        public void WhenIGoTo(string url, int times) {
            for (int i = 0; i != times; ++i)
                DoRequest(url);
        }

        private void DoRequest(string url) {
            DoRequest(url, null);
        }

        private void DoRequest(string url, string postData) {
            _request = (HttpWebRequest)WebRequest.Create("http://localhost" + url);
            _request.CookieContainer = _cookieContainer;
            if (postData != null) {
                _request.Method = "POST";
                _request.ContentType = "application/x-www-form-urlencoded";
                using (var stream = _request.GetRequestStream()) {
                    using (var writer = new StreamWriter(stream)) {
                        writer.Write(postData);
                    }
                }
            }
            try {
                _response = (HttpWebResponse)_request.GetResponse();
            }
            catch (WebException ex) {
                _response = (HttpWebResponse)ex.Response;
            }

            using (var stream = _response.GetResponseStream()) {
                using (var reader = new StreamReader(stream)) {
                    _text = reader.ReadToEnd();
                }
            }
        }
    }
}
