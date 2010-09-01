using System.Web;

namespace Orchard {
    public interface IWorkContextAccessor : IDependency {
        WorkContext GetContext();
        WorkContext GetContext(HttpContextBase httpContext);
    }
}