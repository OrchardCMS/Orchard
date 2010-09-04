using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines.ThemeAwareness {
    public interface ILayoutAwareViewEngine : IDependency, IViewEngine {
    }

    public class LayoutAwareViewEngine : ILayoutAwareViewEngine {
        private readonly IThemeAwareViewEngine _themeAwareViewEngine;

        public LayoutAwareViewEngine(IThemeAwareViewEngine themeAwareViewEngine) {
            _themeAwareViewEngine = themeAwareViewEngine;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {
            return _themeAwareViewEngine.FindPartialView(controllerContext, partialViewName, useCache, true);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {
            var bodyView = _themeAwareViewEngine.FindPartialView(controllerContext, viewName, useCache, true);
            var layoutView = _themeAwareViewEngine.FindPartialView(controllerContext, "Layout", useCache, true);
            var documentView = _themeAwareViewEngine.FindPartialView(controllerContext, "Document", useCache, true);

            if (bodyView.View == null ||
                layoutView.View == null ||
                documentView.View == null) {

                var missingTemplatesResult = new ViewEngineResult(
                    (bodyView.SearchedLocations ?? Enumerable.Empty<string>())
                        .Concat((layoutView.SearchedLocations ?? Enumerable.Empty<string>()))
                        .Concat((documentView.SearchedLocations ?? Enumerable.Empty<string>()))
                    );

                return missingTemplatesResult;
            }

            var view = new LayoutView(new[] {
                bodyView,
                layoutView,
                documentView,
            });

            return new ViewEngineResult(view, this);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view) {
            var layoutView = (LayoutView)view;
            layoutView.ReleaseViews(controllerContext);
        }

        class LayoutView : IView {
            private readonly ViewEngineResult[] _viewEngineResults;

            public LayoutView(ViewEngineResult[] viewEngineResults) {
                _viewEngineResults = viewEngineResults;
            }

            public void Render(ViewContext viewContext, TextWriter writer) {
                var layoutViewContext = LayoutViewContext.From(viewContext);

                for (var index = 0; index != _viewEngineResults.Length; ++index) {
                    bool isFirst = index == 0;
                    bool isLast = index == _viewEngineResults.Length - 1;

                    var effectiveWriter = isLast ? viewContext.Writer : new StringWriter();
                    var effectiveViewData = isFirst ? viewContext.ViewData : CoerceViewData(viewContext.ViewData);
                    var viewEngineResult = _viewEngineResults[index];

                    var effectiveContext = new ViewContext(
                            viewContext,
                            viewEngineResult.View,
                            effectiveViewData,
                            viewContext.TempData,
                            effectiveWriter);

                    viewEngineResult.View.Render(effectiveContext, effectiveWriter);

                    if (!isLast)
                        layoutViewContext.BodyContent = effectiveWriter.ToString();
                }
            }

            private static ViewDataDictionary CoerceViewData(ViewDataDictionary dictionary) {
                return dictionary;
            }

            public void ReleaseViews(ControllerContext context) {
                foreach (var viewEngineResult in _viewEngineResults) {
                    viewEngineResult.ViewEngine.ReleaseView(context, viewEngineResult.View);
                }
            }
        }
    }
}
