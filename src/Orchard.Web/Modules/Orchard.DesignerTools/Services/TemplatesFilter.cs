using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Orchard.DesignerTools.Services {
    public class TemplatesFilter : FilterProvider, IResultFilter {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public TemplatesFilter(
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
                return;

            var ctx = _workContextAccessor.GetContext();
            var tail = ctx.Layout.Tail;
            tail.Add(_shapeFactory.ShapeTracingTemplates());
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}