using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Module=Autofac.Builder.Module;

namespace Orchard.Localization {
    public class LocalizationModule : Module{
        protected override void AttachToComponentRegistration(Autofac.IContainer container, Autofac.IComponentRegistration registration) {

            var userProperty = FindUserProperty(registration.Descriptor.BestKnownImplementationType);

            if (userProperty != null) {
                registration.Activated += (sender, e) => {
                //var authenticationService = e.Context.Resolve<IAuthenticationService>();
                //var currentUser = authenticationService.GetAuthenticatedUser();

                    var text = e.Context.Resolve<IText>();
                    var textDelegate = new Localizer(text.Get);
                    userProperty.SetValue(e.Instance, textDelegate, null);
                };
            }
        }

        private static PropertyInfo FindUserProperty(Type type) {
            return type.GetProperty("T", typeof(Localizer));
        }
    }
}
