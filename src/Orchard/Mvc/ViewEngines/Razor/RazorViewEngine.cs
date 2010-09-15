using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines.Razor {
    public class RazorViewEngine : CshtmlViewEngine {
        [DebuggerStepThrough]
        protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
            if (!virtualPath.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return base.FileExists(controllerContext, virtualPath);
        }
    }
}
