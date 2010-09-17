using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;
using Orchard.Mvc.Html;
using Orchard.Mvc.ViewEngines;

namespace Orchard.UI.Resources {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ResourceFilter(IResourceManager resourceManager, IWorkContextAccessor workContextAccessor, IShapeHelperFactory shapeHelperFactory) {
            _resourceManager = resourceManager;
            _workContextAccessor = workContextAccessor;
            Shape = shapeHelperFactory.CreateHelper();
        }

        private dynamic Shape { get; set; }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var ctx = _workContextAccessor.GetContext();
            var head = ctx.Page.Head;
            var tail = ctx.Page.Tail;
            head.Add(Shape.Metas().ResourceManager(_resourceManager));
            head.Add(Shape.HeadLinks().ResourceManager(_resourceManager));
            head.Add(Shape.StylesheetLinks().ResourceManager(_resourceManager));
            head.Add(Shape.HeadScripts().ResourceManager(_resourceManager));
            tail.Add(Shape.FootScripts().ResourceManager(_resourceManager));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}