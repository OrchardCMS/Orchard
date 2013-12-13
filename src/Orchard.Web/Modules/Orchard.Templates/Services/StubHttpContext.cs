using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using System.Web.ClientServices;
using System.Web.Routing;
using System.Web.Security;

namespace Orchard.Templates.Services {
    internal class StubHttpContext : HttpContextBase {
        private IDictionary _items;
        private HttpRequestBase _request;
        private HttpSessionStateBase _session;
        private IPrincipal _user;

        public override IDictionary Items {
            get { return _items ?? (_items = new Hashtable()); }
        }

        public override HttpRequestBase Request {
            get { return _request ?? (_request = new StubHttpRequest(this)); }
        }

        public override HttpSessionStateBase Session {
            get { return _session ?? (_session = new StubHttpSessionState()); }
        }

        public override IPrincipal User {
            get { return _user ?? (_user = new ClientRolePrincipal(new FormsIdentity(new FormsAuthenticationTicket(FormsAuthentication.FormsCookieName, true, int.MaxValue)))); }
            set { _user = value; }
        }

        public override IHttpHandler Handler { get; set; }

        private class StubHttpRequest : HttpRequestBase {
            private readonly HttpContextBase _httpContext;
            private RequestContext _requestContext;
            private NameValueCollection _queryString;
            private NameValueCollection _headers;

            public StubHttpRequest(HttpContextBase httpContext) {
                _httpContext = httpContext;
            }

            public override RequestContext RequestContext {
                get { return _requestContext ?? (_requestContext = new StubRequestContext(_httpContext)); }
                set { _requestContext = value; }
            }

            public override NameValueCollection QueryString {
                get { return _queryString ?? (_queryString = new NameValueCollection()); }
            }

            public override NameValueCollection Headers {
                get { return _headers ?? (_headers = new NameValueCollection()); }
            }

            public override bool IsAuthenticated {
                get { return true; }
            }

            public override string AppRelativeCurrentExecutionFilePath {
                get { return "/"; }
            }
        }

        private class StubRequestContext : RequestContext {
            private HttpContextBase _httpContext;

            public StubRequestContext(HttpContextBase httpContext) {
                _httpContext = httpContext;
            }

            public override HttpContextBase HttpContext {
                get { return _httpContext; }
                set { _httpContext = value; }
            }
        }

        private class StubHttpSessionState : HttpSessionStateBase {
            private readonly Hashtable _stub = new Hashtable();
            public override object this[int index] {
                get { return _stub[index]; }
                set { _stub[index] = value; }
            }

            public override object this[string index] {
                get { return _stub[index]; }
                set { _stub[index] = value; }
            }

            public override int Count {
                get { return _stub.Count; }
            }

            public override void Clear() {
                _stub.Clear();
            }
        }
    }
}