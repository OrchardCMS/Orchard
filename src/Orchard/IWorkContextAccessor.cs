using System;
using System.Web;

namespace Orchard {
    public interface IWorkContextAccessor : ISingletonDependency {
        WorkContext GetContext(HttpContextBase httpContext);
        IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext);

        WorkContext GetContext();
        IWorkContextScope CreateWorkContextScope();
    }
    
    public interface IWorkContextStateProvider : IDependency {
        T Get<T>(string name);
    }

    public interface IWorkContextScope : IDisposable {
        WorkContext WorkContext { get; }
        TService Resolve<TService>();
    }
}
