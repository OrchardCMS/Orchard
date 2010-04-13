using System.IO;
using System.Web.Mvc;
using Orchard.Mvc.ViewModels;

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
        }

        private static ViewDataDictionary CoerceViewData(ViewDataDictionary dictionary) {
            if (dictionary.Model is BaseViewModel)
                return dictionary;

            return new ViewDataDictionary<BaseViewModel>(BaseViewModel.From(dictionary));
        }



        public void ReleaseViews(ControllerContext context) {
            foreach (var viewEngineResult in _viewEngineResults) {
                viewEngineResult.ViewEngine.ReleaseView(context, viewEngineResult.View);
            }
        }
    }
}
