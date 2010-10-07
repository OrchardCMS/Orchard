using System.Web.Mvc;
using Orchard.Mvc.Filters;

namespace Orchard.Search.Filters {
    public class SearchFilter : FilterProvider, IResultFilter {
        private readonly IWorkContextAccessor _workContextAccessor;

        public SearchFilter(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            dynamic search = filterContext.Controller.ViewData.Model;
            var workContext = _workContextAccessor.GetContext(filterContext);

            if (search != null)
                workContext.Layout.Search.Add(search);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}