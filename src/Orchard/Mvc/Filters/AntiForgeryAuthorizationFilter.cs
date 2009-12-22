using System.Web.Mvc;

namespace Orchard.Mvc.Filters {
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        public void OnAuthorization(AuthorizationContext filterContext) {
            if (!(filterContext.HttpContext.Request.HttpMethod == "POST" && filterContext.RequestContext.HttpContext.Request.IsAuthenticated))
                return;

            //TODO: (erikpo) Change the salt to be something unique per application like a site setting with a Guid.NewGuid().ToString("N") value
            ValidateAntiForgeryTokenAttribute validator = new ValidateAntiForgeryTokenAttribute { Salt = "Orchard" };

            validator.OnAuthorization(filterContext);
        }
    }
}