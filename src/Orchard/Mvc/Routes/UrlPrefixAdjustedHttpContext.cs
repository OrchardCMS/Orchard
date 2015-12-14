using System.Web;
using System.Web.SessionState;
using Orchard.Mvc.Wrappers;

namespace Orchard.Mvc.Routes {
    public class UrlPrefixAdjustedHttpContext : HttpContextBaseWrapper {
        private readonly UrlPrefix _prefix;

        public UrlPrefixAdjustedHttpContext(HttpContextBase httpContextBase, UrlPrefix prefix)
            : base(httpContextBase) {
            _prefix = prefix;
        }

        public override HttpRequestBase Request {
            get {
                return new AdjustedRequest(_httpContextBase.Request, _prefix);
            }
        }

        public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior)
        {
            _httpContextBase.SetSessionStateBehavior(sessionStateBehavior);
        }

        class AdjustedRequest : HttpRequestBaseWrapper {
            private readonly UrlPrefix _prefix;

            public AdjustedRequest(HttpRequestBase httpRequestBase, UrlPrefix prefix)
                : base(httpRequestBase) {
                _prefix = prefix;
            }

            public override string AppRelativeCurrentExecutionFilePath {
                get {
                    return _prefix.RemoveLeadingSegments(_httpRequestBase.AppRelativeCurrentExecutionFilePath);
                }
            }
        }

    }
}
