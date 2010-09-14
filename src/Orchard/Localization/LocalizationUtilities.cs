using System.Web.Mvc;
using Autofac;
using Orchard.Mvc;

namespace Orchard.Localization {
    public class LocalizationUtilities {
        public static Localizer Resolve(ControllerContext controllerContext, string scope) {
            var context = controllerContext.GetWorkContext();
            return context == null ? NullLocalizer.Instance : Resolve(context.Resolve<ILifetimeScope>(), scope);
        }

        public static Localizer Resolve(IComponentContext context, string scope) {
            var text = context.Resolve<IText>(new NamedParameter("scope", scope));
            return text.Get;
        }
    }
}
