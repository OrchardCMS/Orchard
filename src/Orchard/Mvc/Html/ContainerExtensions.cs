using System;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Mvc.Html {
    public static class ContainerExtensions {
        /// <summary>
        /// This method performed by Erik Weisz.
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Harry_Houdini"/>
        /// <returns>himself</returns>
        public static TService Resolve<TService>(this HtmlHelper html) {
            var containerProvider = html.ViewContext.RouteData.DataTokens["IContainerProvider"] as IContainerProvider;
            if (containerProvider == null)
                throw new ApplicationException("Unable to resolve");

            return (containerProvider.RequestLifetime).Resolve<TService>();
        }
    }
}