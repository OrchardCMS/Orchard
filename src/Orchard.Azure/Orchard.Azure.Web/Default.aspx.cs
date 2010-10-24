using System;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Orchard.Azure.Web {
    public class _Default : Page { 
        public void Page_Load(object sender, EventArgs e) {
            // Change the current path so that the Routing handler can correctly interpret
            // the request, then restore the original path so that the OutputCache module
            // can correctly process the response (if caching is enabled).

            var originalPath = Request.Path;
            HttpContext.Current.RewritePath(Request.ApplicationPath, false);
            IHttpHandler httpHandler = new MvcHttpHandler();
            httpHandler.ProcessRequest(HttpContext.Current);
            HttpContext.Current.RewritePath(originalPath, false);
        }
    }
}