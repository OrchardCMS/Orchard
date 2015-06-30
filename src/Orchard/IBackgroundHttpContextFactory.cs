using System.Web;

namespace Orchard {
    public interface IBackgroundHttpContextFactory : IDependency {
        HttpContext CreateHttpContext();
        void InitializeHttpContext();
    }
}
