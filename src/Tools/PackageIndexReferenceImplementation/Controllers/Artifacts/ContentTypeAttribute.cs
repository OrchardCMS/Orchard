using System;
using System.Reflection;
using System.Web.Mvc;

namespace PackageIndexReferenceImplementation.Controllers.Artifacts {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ContentTypeAttribute : ActionMethodSelectorAttribute {
        public ContentTypeAttribute(string contentType) {
            ContentType = contentType;
        }

        public string ContentType { get; set; }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
            return controllerContext.HttpContext.Request.ContentType.StartsWith(ContentType);
        }
    }
}