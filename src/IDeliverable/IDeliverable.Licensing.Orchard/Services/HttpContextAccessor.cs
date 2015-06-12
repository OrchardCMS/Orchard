using System.Web;

namespace IDeliverable.Licensing.Orchard.Services
{
    public class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpContextBase Current()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}