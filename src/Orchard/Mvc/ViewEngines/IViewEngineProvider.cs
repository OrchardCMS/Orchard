using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class CreateThemeViewEngineParams {
        public string VirtualPath { get; set; }
    }

    public class CreatePackagesViewEngineParams {
        public IEnumerable<string> VirtualPaths { get; set; }
    }

    public interface IViewEngineProvider : IDependency {
        IViewEngine CreateThemeViewEngine(CreateThemeViewEngineParams parameters);
        IViewEngine CreatePackagesViewEngine(CreatePackagesViewEngineParams parameters);
    }
}
