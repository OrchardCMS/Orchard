using System.Web;
using Autofac;

namespace Orchard.Mvc {
    public interface IHttpContextAccessor {
        HttpContextBase Current();
        HttpContextBase CreateContext(ILifetimeScope lifetimeScope);
    }
}
