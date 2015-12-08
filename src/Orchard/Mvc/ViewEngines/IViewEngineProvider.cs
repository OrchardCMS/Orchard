using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class CreateThemeViewEngineParams {
        public string VirtualPath { get; set; }
    }

    public class CreateModulesViewEngineParams {
        public IEnumerable<string> VirtualPaths { get; set; }
    }

    public interface IViewEngineProvider : ISingletonDependency {
        IViewEngine CreateThemeViewEngine(CreateThemeViewEngineParams parameters);
        IViewEngine CreateModulesViewEngine(CreateModulesViewEngineParams parameters);

        /// <summary>
        /// Produce a view engine configured to resolve only fully qualified {viewName} parameters
        /// </summary>
        IViewEngine CreateBareViewEngine();
    }
}
