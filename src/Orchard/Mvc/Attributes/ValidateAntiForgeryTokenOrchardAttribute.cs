using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateAntiForgeryTokenOrchardAttribute : FilterAttribute {
    }
}