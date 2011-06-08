using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using IFilterProvider = Orchard.Mvc.Filters.IFilterProvider;

namespace Orchard.Exceptions.Filters {
    public class UnhandledExceptionFilter : FilterProvider, IActionFilter {
        private readonly IOrchardServices _orchardServices;
        private readonly Lazy<IEnumerable<IFilterProvider>> _filterProviders;

        public UnhandledExceptionFilter(
            IOrchardServices orchardServices,
            Lazy<IEnumerable<IFilterProvider>> filters) {
            _orchardServices = orchardServices;
            _filterProviders = filters;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            if (!filterContext.ExceptionHandled && filterContext.Exception != null) {
                var shape = _orchardServices.New.ErrorPage();
                shape.Message = filterContext.Exception.Message;
                shape.Exception = filterContext.Exception;
                Logger.Error(filterContext.Exception.Message);

                filterContext.ExceptionHandled = true;

                // inform exception filters of the exception that was suppressed
                var filterInfo = new FilterInfo();
                foreach (var filterProvider in _filterProviders.Value) {
                    filterProvider.AddFilters(filterInfo);
                }

                var exceptionContext = new ExceptionContext(filterContext.Controller.ControllerContext, filterContext.Exception);
                foreach (var exceptionFilter in filterInfo.ExceptionFilters) {
                    exceptionFilter.OnException(exceptionContext);
                }

                if (exceptionContext.ExceptionHandled) {
                    filterContext.Result = exceptionContext.Result;
                }
                else {
                    filterContext.Result = new ShapeResult(filterContext.Controller, shape);
                    filterContext.RequestContext.HttpContext.Response.StatusCode = 500;
                }
            }
        }
    }
}
