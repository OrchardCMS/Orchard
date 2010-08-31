using System;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines.WebForms {
    public class WebFormViewEngineForAspNet4 : WebFormViewEngine {
        protected override bool FileExists(ControllerContext controllerContext, string virtualPath) {
            try {
                if (virtualPath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase) ||
                    virtualPath.EndsWith(".ascx", StringComparison.InvariantCultureIgnoreCase) ||
                    virtualPath.EndsWith(".master", StringComparison.InvariantCultureIgnoreCase)) {
                    return BuildManager.GetObjectFactory(virtualPath, false) != null;
                }
                return false;
            }
            catch (HttpException exception) {
                // Reproducing base class behavior, however these this code path should
                // not be followed for "not found" cases
                if (exception is HttpParseException) {
                    throw;
                }
                if ((exception.GetHttpCode() != 0x194) || base.FileExists(controllerContext, virtualPath)) {
                    throw;
                }
                return false;
            }
        }
    }
}
