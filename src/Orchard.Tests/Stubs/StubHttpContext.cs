using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Orchard.Tests.Stubs {
    public class StubHttpContext : HttpContextBase {
        private readonly string _appRelativeCurrentExecutionFilePath;
        private readonly string _hostHeader;
        private readonly IDictionary _items = new Dictionary<object, object>();

        public StubHttpContext() {
            _appRelativeCurrentExecutionFilePath = "~/yadda";
        }

        public StubHttpContext(string appRelativeCurrentExecutionFilePath) {
            _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
        }

        public StubHttpContext(string appRelativeCurrentExecutionFilePath, string hostHeader) {
            _appRelativeCurrentExecutionFilePath = appRelativeCurrentExecutionFilePath;
            _hostHeader = hostHeader;
        }

        public override HttpRequestBase Request {
            get { return new StubHttpRequest(this); }
        }
        public override HttpResponseBase Response {
            get { return new StubHttpResponse(this); }
        }


        public override IDictionary Items {
            get { return _items; }
        }

        class StubHttpRequest : HttpRequestBase {
            private readonly StubHttpContext _httpContext;
            private NameValueCollection _serverVariables;

            public StubHttpRequest(StubHttpContext httpContext) {
                _httpContext = httpContext;
            }

            public override string AppRelativeCurrentExecutionFilePath {
                get { return _httpContext._appRelativeCurrentExecutionFilePath; }
            }

            public override string ApplicationPath {
                get { return "/"; }
            }

            public override string PathInfo {
                get { return ""; }
            }

            public override NameValueCollection ServerVariables {
                get {
                    return _serverVariables = _serverVariables
                        ?? new NameValueCollection { { "HTTP_HOST", _httpContext._hostHeader } };
                }
            }
        }

        class StubHttpResponse : HttpResponseBase {
            private readonly StubHttpContext _httpContext;

            public StubHttpResponse(StubHttpContext httpContext) {
                _httpContext = httpContext;
            }
            public override string ApplyAppPathModifier(string virtualPath) {
                return "~/" + virtualPath.TrimStart('/');
            }
        }

    }
}