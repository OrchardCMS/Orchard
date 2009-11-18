using System;
using System.Reflection;
using Module = Autofac.Builder.Module;

namespace Orchard.Settings {
    public class SettingsModule : Module {
        protected override void AttachToComponentRegistration(Autofac.IContainer container, Autofac.IComponentRegistration registration) {

            var siteProperty = FindSiteProperty(registration.Descriptor.BestKnownImplementationType);

            if (siteProperty != null) {
                registration.Activated += (sender, e) => {
                    var siteService = e.Context.Resolve<ISiteService>();
                    var currentSite = siteService.GetSiteSettings();
                    siteProperty.SetValue(e.Instance, currentSite, null);
                };
            }
        }

        private static PropertyInfo FindSiteProperty(Type type) {
            return type.GetProperty("CurrentSite", typeof(ISite));
        }
    }
}
