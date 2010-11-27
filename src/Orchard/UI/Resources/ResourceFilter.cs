using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Orchard.UI.Resources {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public ResourceFilter(
            IResourceManager resourceManager, 
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _resourceManager = resourceManager;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }


        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
                return;

            var ctx = _workContextAccessor.GetContext();
            var head = ctx.Layout.Head;
            var tail = ctx.Layout.Tail;
            head.Add(_shapeFactory.Metas().ResourceManager(_resourceManager));
            head.Add(_shapeFactory.HeadLinks().ResourceManager(_resourceManager));
            head.Add(_shapeFactory.StylesheetLinks().ResourceManager(_resourceManager));
            head.Add(_shapeFactory.HeadScripts().ResourceManager(_resourceManager));
            tail.Add(_shapeFactory.FootScripts().ResourceManager(_resourceManager));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}