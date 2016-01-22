using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Mvc.Spooling;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Mvc.ViewEngines.ThemeAwareness {
    public interface ILayoutAwareViewEngine : IDependency, IViewEngine {
    }

    public class LayoutAwareViewEngine : ILayoutAwareViewEngine {
        private readonly WorkContext _workContext;
        private readonly IThemeAwareViewEngine _themeAwareViewEngine;
        private readonly IDisplayHelperFactory _displayHelperFactory;

        public LayoutAwareViewEngine(
            WorkContext workContext,
            IThemeAwareViewEngine themeAwareViewEngine,
            IDisplayHelperFactory displayHelperFactory) {
            _workContext = workContext;
            _themeAwareViewEngine = themeAwareViewEngine;
            _displayHelperFactory = displayHelperFactory;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {
            return _themeAwareViewEngine.FindPartialView(controllerContext, partialViewName, useCache, true);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {
            var viewResult = _themeAwareViewEngine.FindPartialView(controllerContext, viewName, useCache, true);

            if (viewResult.View == null) {
                return viewResult;
            }

            if (!ThemeFilter.IsApplied(controllerContext.RequestContext)) {
                return viewResult;
            }

            var layoutView = new LayoutView((viewContext, writer, viewDataContainer) => {
                Logger.Information("Rendering layout view");

                var childContentWriter = new HtmlStringWriter();

                var childContentViewContext = new ViewContext(
                    viewContext, 
                    viewContext.View, 
                    viewContext.ViewData, 
                    viewContext.TempData,
                    childContentWriter);

                viewResult.View.Render(childContentViewContext, childContentWriter);
                _workContext.Layout.Metadata.ChildContent = childContentWriter;

                var display = _displayHelperFactory.CreateHelper(viewContext, viewDataContainer);
                IHtmlString result = display(_workContext.Layout);
                writer.Write(result.ToHtmlString());

                Logger.Information("Done rendering layout view");
            }, (context, view) => viewResult.ViewEngine.ReleaseView(context, viewResult.View));

            return new ViewEngineResult(layoutView, this);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view) {
            var layoutView = (LayoutView)view;
            layoutView.ReleaseView(controllerContext, view);
        }

        class LayoutView : IView, IViewDataContainer {
            private readonly Action<ViewContext, TextWriter, IViewDataContainer> _render;
            private readonly Action<ControllerContext, IView> _releaseView;

            public LayoutView(Action<ViewContext, TextWriter, IViewDataContainer> render, Action<ControllerContext, IView> releaseView) {
                _render = render;
                _releaseView = releaseView;
            }

            public ViewDataDictionary ViewData { get; set; }

            public void Render(ViewContext viewContext, TextWriter writer) {
                ViewData = viewContext.ViewData;
                _render(viewContext, writer, this);
            }

            public void ReleaseView(ControllerContext context, IView view) {
                _releaseView(context, view);
            }
        }
    }
}
