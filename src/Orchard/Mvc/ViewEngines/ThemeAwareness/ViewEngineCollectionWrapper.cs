using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines.ThemeAwareness {
    public class ViewEngineCollectionWrapper : IViewEngine {
        private readonly IEnumerable<IViewEngine> _engines;

        public ViewEngineCollectionWrapper(IEnumerable<IViewEngine> engines) {
            _engines = engines.ToArray();
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {
            var searchedLocations = Enumerable.Empty<string>();
            foreach (var engine in _engines) {
                var result = engine.FindPartialView(controllerContext, partialViewName, useCache);
                if (result.View != null)
                    return result;
                if (!useCache)
                    searchedLocations = searchedLocations.Concat(result.SearchedLocations);
            }
            return new ViewEngineResult(searchedLocations.Distinct());
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {
            var searchedLocations = Enumerable.Empty<string>();
            foreach (var engine in _engines) {
                var result = engine.FindView(controllerContext, viewName, masterName, useCache);
                if (result.View != null)
                    return result;
                if (!useCache)
                    searchedLocations = searchedLocations.Concat(result.SearchedLocations);
            }
            return new ViewEngineResult(searchedLocations.Distinct());
        }

        public void ReleaseView(ControllerContext controllerContext, IView view) {
            throw new NotImplementedException();
        }
    }
}
