using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class LayoutViewEngine : IViewEngine {
        private readonly ViewEngineCollection _viewEngines;

        public LayoutViewEngine(ViewEngineCollection viewEngines) {
            _viewEngines = viewEngines;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {
            return new ViewEngineResult(Enumerable.Empty<string>());
        }

        public ViewEngineResult FindView(
            ControllerContext controllerContext,
            string viewName,
            string masterName,
            bool useCache) {

            // TODO: is there an optimization around useCache for
            // this implementation? since IView can't re-execute, maybe not...

            // if action returned a View with explicit master - 
            // this will bypass the multi-pass layout strategy
            if (!string.IsNullOrEmpty(masterName))
                return new ViewEngineResult(Enumerable.Empty<string>());

            var bodyView = _viewEngines.FindPartialView(controllerContext, viewName);
            var layoutView = _viewEngines.FindPartialView(controllerContext, "layout");
            var documentView = _viewEngines.FindPartialView(controllerContext, "document");

            if (bodyView.View == null ||
                layoutView.View == null ||
                documentView.View == null) {
             
                return new ViewEngineResult(
                    (bodyView.SearchedLocations ?? Enumerable.Empty<string>())
                        .Concat((layoutView.SearchedLocations ?? Enumerable.Empty<string>()))
                        .Concat((documentView.SearchedLocations ?? Enumerable.Empty<string>()))
                    );
            }

            var view = new LayoutView(new[] {
                                                bodyView.View,
                                                layoutView.View,
                                                documentView.View,
                                            });

            return new ViewEngineResult(view, this);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view) {

        }
    }


}
