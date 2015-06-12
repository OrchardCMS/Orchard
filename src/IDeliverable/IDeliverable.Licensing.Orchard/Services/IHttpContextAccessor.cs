using System.Web;

namespace IDeliverable.Licensing.Orchard.Services
{
    public interface IHttpContextAccessor
    {
        HttpContextBase Current();
    }
}