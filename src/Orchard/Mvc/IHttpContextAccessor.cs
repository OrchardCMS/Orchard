using System.Web;

namespace Orchard.Mvc {
    public interface IHttpContextAccessor {
        HttpContextBase Current();
    }
}
