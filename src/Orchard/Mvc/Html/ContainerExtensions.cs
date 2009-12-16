using System;
using System.Web.Mvc;
using Autofac.Integration.Web;

namespace Orchard.Mvc.Html {
    public static class ContainerExtensions {
        public static TService Resolve<TService>(this HtmlHelper html) {
            var containerProvider = html.ViewContext.RouteData.DataTokens["IContainerProvider"] as IContainerProvider;
            if (containerProvider == null)
                throw new ApplicationException("Unable to resolve");

            return containerProvider.RequestContainer.Resolve<TService>();
        }
    }
}