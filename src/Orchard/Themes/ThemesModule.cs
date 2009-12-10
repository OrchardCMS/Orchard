using System;
using System.Reflection;
using Module = Autofac.Builder.Module;

namespace Orchard.Themes {
    public class ThemesModule : Module {
        protected override void AttachToComponentRegistration(Autofac.IContainer container, Autofac.IComponentRegistration registration) {

            var themeProperty = FindThemeProperty(registration.Descriptor.BestKnownImplementationType);

            if (themeProperty != null) {
                registration.Activated += (sender, e) => {
                    var themeService = e.Context.Resolve<IThemeService>();
                    var currentTheme = themeService.GetCurrentTheme();
                    themeProperty.SetValue(e.Instance, currentTheme, null);
                };
            }
        }

        private static PropertyInfo FindThemeProperty(Type type) {
            return type.GetProperty("CurrentTheme", typeof(ITheme));
        }
    }
}
