using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class WebFormsViewEngineProvider : IViewEngineProvider {
        public IViewEngine CreateThemeViewEngine(CreateThemeViewEngineParams parameters) {
            //var viewEngine = new WebFormViewEngine();
            //viewEngine.PartialViewLocationFormats
            return null;
        }

        public IViewEngine CreatePackagesViewEngine(CreatePackagesViewEngineParams parameters) {
            return null; 
        }
    }
}
