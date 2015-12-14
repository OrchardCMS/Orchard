using System.Web;

namespace Orchard.Web.Tests.Stubs {
    internal class StubContext : HttpContextBase {
        private readonly StubRequest request;

        public StubContext(string relativeUrl) {
            request = new StubRequest(relativeUrl);
        }

        public override HttpRequestBase Request {
            get { return request; }
        }
    }
}