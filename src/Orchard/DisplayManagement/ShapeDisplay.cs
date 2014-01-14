using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public class ShapeDisplay : IShapeDisplay {
        private readonly IDisplayHelperFactory _displayHelperFactory;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly HttpContextBase _httpContextBase;
        private readonly RequestContext _requestContext;

        public ShapeDisplay(
            IDisplayHelperFactory displayHelperFactory, 
            IWorkContextAccessor workContextAccessor, 
            HttpContextBase httpContextBase,
            RequestContext requestContext) {
            _displayHelperFactory = displayHelperFactory;
            _workContextAccessor = workContextAccessor;
            _httpContextBase = httpContextBase;
            _requestContext = requestContext;
        }

        public string Display(Shape shape) {
            return Display((object) shape);
        }

        public string Display(object shape) {
            var viewContext = new ViewContext {
                HttpContext = _httpContextBase, 
                RequestContext = _requestContext
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