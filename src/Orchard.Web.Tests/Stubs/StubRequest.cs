using System.Web;

namespace Orchard.Web.Tests.Stubs {
    public class StubRequest : HttpRequestBase {
        private readonly string relativeUrl;

        public StubRequest(string relativeUrl) {
            this.relativeUrl = relativeUrl;
        }

        public override string AppRelativeCurrentExecutionFilePath {
            get { return relativeUrl; }
        }

        public override string PathInfo {
            get { return ""; }
        }
    }
}