using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines.Razor {
    public class RazorViewEngine : CshtmlViewEngine {
        protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
            if (!virtualPath.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return base.FileExists(controllerContext, virtualPath);
        }
    }
}
