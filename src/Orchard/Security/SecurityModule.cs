using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Module=Autofac.Builder.Module;

namespace Orchard.Security {
    public class SecurityModule : Module {
        protected override void AttachToComponentRegistration(Autofac.IContainer container, Autofac.IComponentRegistration registration) {

            var userProperty = FindUserProperty(registration.Descriptor.BestKnownImplementationType);

            if (userProperty != null) {
                registration.Activated += (sender, e) => {
                                              var authenticationService = e.Context.Resolve<IAuthenticationService>();
                                              var currentUser = authenticationService.Authenticated();
                                              userProperty.SetValue(e.Instance, currentUser, null);
                                          };
            }
        }

        private static PropertyInfo FindUserProperty(Type type) {
            return type.GetProperty("CurrentUser", typeof (IUser));
        }
    }
}
