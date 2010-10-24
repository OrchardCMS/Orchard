using System.Web.Mvc;
using Autofac;

namespace Orchard.Localization {
    public class LocalizationUtilities {
        public static Localizer Resolve(WorkContext workContext, string scope) {
            return workContext == null ? NullLocalizer.Instance : Resolve(workContext.Resolve<ILifetimeScope>(), scope);
        }

        public static Localizer Resolve(ControllerContext controllerContext, string scope) {
            var workContext = controllerContext.GetWorkContext();
            return Resolve(workContext, scope);
        }

        public static Localizer Resolve(IComponentContext context, string scope) {
            var text = context.Resolve<IText>(new NamedParameter("scope", scope));
            return text.Get;
        }
    }
}
