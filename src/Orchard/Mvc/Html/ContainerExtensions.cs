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
            var workContext = html.ViewContext.RequestContext.GetWorkContext();

            if (workContext == null)
                return default(TService);

            return workContext.Resolve<TService>();
        }
    }
}