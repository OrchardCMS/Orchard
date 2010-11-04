using System;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Orchard.Environment.Extensions.Models;
using Module = Autofac.Module;

namespace Orchard.Themes {
    public class ThemesModule : Module {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            var themeProperty = FindThemeProperty(registration.Activator.LimitType);
            
            if (themeProperty != null) {
                registration.Activated += (sender, e) => {
                    var themeService = e.Context.Resolve<IThemeService>();
                    var currentTheme = themeService.GetSiteTheme();
                    themeProperty.SetValue(e.Instance, currentTheme, null);
                };
            }
        }

        private static PropertyInfo FindThemeProperty(Type type) {
            return type.GetProperty("CurrentTheme", typeof(FeatureDescriptor));
        }
    }
}
