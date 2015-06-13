using System.Web;

namespace IDeliverable.Licensing
{
    public class HttpContextAccessor
    {
        public HttpContextBase Current()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}