using System.Web.Mvc;

namespace Orchard.Mvc.Filters {
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        public void OnAuthorization(AuthorizationContext filterContext) {
            //TODO: (erikpo) Once all modules are moved over to use the AntiForgeryToken, get rid of this if statement
            if (!(filterContext.RouteData.Values["area"] is string
                && (string)filterContext.RouteData.Values["area"] == "Orchard.Blogs"))
                return;
            
            if (!(filterContext.HttpContext.Request.HttpMethod == "POST" && filterContext.RequestContext.HttpContext.Request.IsAuthenticated))
                return;

            ValidateAntiForgeryTokenAttribute validator = new ValidateAntiForgeryTokenAttribute { Salt = "Orchard" };

            validator.OnAuthorization(filterContext);
        }
    }
}