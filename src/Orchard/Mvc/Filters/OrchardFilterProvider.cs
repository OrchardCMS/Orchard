using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Mvc.Filters {
    public class OrchardFilterProvider : System.Web.Mvc.IFilterProvider {

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor) {
            var workContext = controllerContext.GetWorkContext();

            // Map IFilterProvider implementations to MVC Filter objects
            // Need to provide order values since Filter objects of identical
            // scope and order would run in undefined order.
            // We create negative order values to avoid conflicts with other
            // potential user-provided MVC Filter objects, which hopefully use
            // positive order values. We do this by reversing the list and
            // negating the index.
            var filters = workContext.Resolve<IEnumerable<IFilterProvider>>();
            return filters.Reverse().Select((filter, index) => new Filter(filter, FilterScope.Action, -(index + 1)));
        }
    }
}