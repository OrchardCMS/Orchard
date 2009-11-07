using System.Web;

namespace Orchard.Tests.Stubs {
    public class StubHttpContext : HttpContextBase {
        private readonly string _appRelativeCurrentExecutionFilePath;

        public StubHttpContext() {
            _appRelativeCurrentExecutionFilePath = "~/yadda";
        }

        public StubHttpContext(string appRelativeCurrentExecutionFilePath) {
            _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
        }

        public override HttpRequestBase Request {
            get { return new StupHttpRequest(_appRelativeCurrentExecutionFilePath); }
        }

        public class StupHttpRequest : HttpRequestBase {
            private readonly string _appRelativeCurrentExecutionFilePath;

            public StupHttpRequest(string appRelativeCurrentExecutionFilePath) {
                _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
            }

            public override string AppRelativeCurrentExecutionFilePath {
                get { return _appRelativeCurrentExecutionFilePath; }
            }

            public override string PathInfo {
                get { return ""; }
            }
        }
    }
}