using System;
using System.Web.Mvc;

namespace Orchard.Mvc.AntiForgery {
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateAntiForgeryTokenOrchardAttribute : FilterAttribute {
    }
}