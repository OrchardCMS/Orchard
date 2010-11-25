using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Html {
    public static class ContainerExtensions {
        /// <summary>
        /// This method performed by Erik Weisz.
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Harry_Houdini"/>
        /// <returns>himself</returns>
        public static TService Resolve<TService>(this HtmlHelper html) {
            var workContextAccessor = html.ViewContext.RouteData.DataTokens["IWorkContextAccessor"] as IWorkContextAccessor;
            if (workContextAccessor == null)
                throw new ApplicationException("Unable to resolve");

            var workContext = workContextAccessor.GetContext(html.ViewContext.HttpContext);
            if (workContext == null)
                throw new ApplicationException("Unable to resolve");

            return workContext.Resolve<TService>();
        }
    }
}