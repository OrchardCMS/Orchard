using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Html {
    public interface IFoo{}
    public static class ContainerExtensions {
        /// <summary>
        /// This method performed by Erik Weisz.
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Harry_Houdini"/>
        /// <returns>himself</returns>
        public static TService Resolve<TService>(this HtmlHelper html) {
            var workContext = html.ViewContext.RequestContext.GetWorkContext();

            if (workContext == null)
                throw new ApplicationException(string.Format(@"The WorkContext cannot be found for the request. Unable to resolve '{0}'.", typeof(TService)));

            return workContext.Resolve<TService>();
        }
    }
}