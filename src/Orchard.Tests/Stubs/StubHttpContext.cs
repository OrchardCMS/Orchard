using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace Orchard.Tests.Stubs {
    public class StubHttpContext : HttpContextBase {
        private readonly string _appRelativeCurrentExecutionFilePath;
        private readonly IDictionary _items = new Dictionary<object, object>();

        public StubHttpContext() {
            _appRelativeCurrentExecutionFilePath = "~/yadda";
        }

        public StubHttpContext(string appRelativeCurrentExecutionFilePath) {
            _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
        }

        public override HttpRequestBase Request {
            get { return new StupHttpRequest(_appRelativeCurrentExecutionFilePath); }
        }

        public override IDictionary Items {
            get { return _items; }
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