using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class LayoutView : IView {
        private readonly LayoutViewEngine _viewEngine;
        private ViewEngineResult[] _viewEngineResults;

        public LayoutView(LayoutViewEngine viewEngine, ViewEngineResult[] views) {
            _viewEngine = viewEngine;
            _viewEngineResults = views;
        }

        public void Render(ViewContext viewContext, TextWriter writer) {
            using (_viewEngine.CreateScope(viewContext)) {
                var layoutViewContext = LayoutViewContext.From(viewContext);

                for (var index = 0; index != _viewEngineResults.Length; ++index) {
                    var viewEngineResult = _viewEngineResults[index];
                    if (index == _viewEngineResults.Length - 1) {
                        viewEngineResult.View.Render(viewContext, writer);
                    }
                    else {
                        //TEMP: to be replaced with an efficient spooling writer
                        var childContext = new ViewContext(
                            viewContext,
                            viewEngineResult.View,
                            viewContext.ViewData,
                            viewContext.TempData,
                            new StringWriter());
                        viewEngineResult.View.Render(childContext, childContext.Writer);
                        layoutViewContext.BodyContent = childContext.Writer.ToString();
                    }
                }
            }
        }

        public void ReleaseViews(ControllerContext context) {
            foreach (var viewEngineResult in _viewEngineResults) {
                viewEngineResult.ViewEngine.ReleaseView(context, viewEngineResult.View);
            }
        }
    }
}
