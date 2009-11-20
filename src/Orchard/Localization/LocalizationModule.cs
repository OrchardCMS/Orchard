using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Module = Autofac.Builder.Module;

namespace Orchard.Localization {
    public class LocalizationModule : Module {

        protected override void Load(Autofac.Builder.ContainerBuilder builder) {
            builder.Register<Text>().As<IText>().FactoryScoped();
        }

        protected override void AttachToComponentRegistration(Autofac.IContainer container, Autofac.IComponentRegistration registration) {

            var userProperty = FindUserProperty(registration.Descriptor.BestKnownImplementationType);

            if (userProperty != null) {
                var scope = registration.Descriptor.BestKnownImplementationType.FullName;

                registration.Activated += (sender, e) => {
                    //var authenticationService = e.Context.Resolve<IAuthenticationService>();
                    //var currentUser = authenticationService.GetAuthenticatedUser();

                    var localizer = LocalizationUtilities.Resolve(e.Context, scope);
                    userProperty.SetValue(e.Instance, localizer, null);
                };
            }
        }

        private static PropertyInfo FindUserProperty(Type type) {
            return type.GetProperty("T", typeof(Localizer));
        }
    }
}
