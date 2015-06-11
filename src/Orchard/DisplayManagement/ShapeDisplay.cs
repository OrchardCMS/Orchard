using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public class ShapeDisplay : IShapeDisplay {
        private readonly IDisplayHelperFactory _displayHelperFactory;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ShapeDisplay(
            IDisplayHelperFactory displayHelperFactory,
            IWorkContextAccessor workContextAccessor) {
            _displayHelperFactory = displayHelperFactory;
            _workContextAccessor = workContextAccessor;
        }

        public string Display(Shape shape) {
            return Display((object)shape);
        }

        public string Display(object shape) {
            var workContext = _workContextAccessor.GetContext();
            var httpContext = workContext.HttpContext;
            var viewContext = new ViewContext {
                HttpContext = httpContext,
                RequestContext = httpContext.Request.RequestContext
            };
            viewContext.RouteData.DataTokens["IWorkContextAccessor"] = _workContextAccessor;
            var display = _displayHelperFactory.CreateHelper(viewContext, new ViewDataContainer());

            return ((DisplayHelper)display).ShapeExecute(shape).ToString();
        }

        public IEnumerable<string> Display(IEnumerable<object> shapes) {
            return shapes.Select(Display).ToArray();
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }

            public ViewDataContainer() {
                ViewData = new ViewDataDictionary();
            }
        }
    }
}