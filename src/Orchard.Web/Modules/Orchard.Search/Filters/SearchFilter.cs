using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.Search.ViewModels;

namespace Orchard.Search.Filters {
    public class SearchFilter : FilterProvider, IResultFilter {
        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewModel = filterContext.Controller.ViewData.Model as BaseViewModel;

            if (viewModel != null)
                viewModel.Zones.AddRenderPartial("search", "SearchForm", viewModel is SearchViewModel ? viewModel : new SearchViewModel());
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}