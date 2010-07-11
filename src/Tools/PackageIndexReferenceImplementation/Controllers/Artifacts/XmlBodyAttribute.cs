using System;
using System.Web.Mvc;
using System.Xml.Linq;

namespace PackageIndexReferenceImplementation.Controllers.Artifacts {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class XmlBodyAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var body = XElement.Load(filterContext.HttpContext.Request.InputStream);
            filterContext.ActionParameters["body"] = body.ToString();
        }
    }
}