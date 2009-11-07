using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Mvc.Filters {
    public class FilterResolvingActionInvoker : ControllerActionInvoker {
        private readonly IEnumerable<IFilterProvider> _filterProviders;

        public FilterResolvingActionInvoker(IEnumerable<IFilterProvider> filterProviders) {
            _filterProviders = filterProviders;
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
            var filters= base.GetFilters(controllerContext, actionDescriptor);
            foreach(var provider in _filterProviders) {
                provider.AddFilters(filters);
            }
            return filters;
        }
    }
}