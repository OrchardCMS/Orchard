using System;
using System.Web;

namespace Orchard {
    public interface IWorkContextAccessor : ISingletonDependency {
        WorkContext GetContext(HttpContextBase httpContext);
        IWorkContextScope CreateContextScope(HttpContextBase httpContext);

        WorkContext GetContext();
        IWorkContextScope CreateContextScope();
    }

    public interface IWorkContextScope : IDisposable {
        WorkContext WorkContext { get; }
    }
}
