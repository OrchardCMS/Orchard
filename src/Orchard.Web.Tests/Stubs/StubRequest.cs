using System.Web;

namespace Orchard.Web.Tests.Stubs
{
    public class StubRequest : HttpRequestBase
    {
        private readonly string relativeUrl;

        public StubRequest(string relativeUrl)
        {
            this.relativeUrl = relativeUrl;
        }

        public override string AppRelativeCurrentExecutionFilePath => relativeUrl;

        public override string PathInfo => "";
    }
}