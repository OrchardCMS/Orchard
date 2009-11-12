using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Autofac.Builder;

namespace Orchard.Security {
    public class SecurityModule : Module {
        protected override void AttachToComponentRegistration(Autofac.IContainer container, Autofac.IComponentRegistration registration) {
            if (typeof(ICurrentUser).IsAssignableFrom(registration.Descriptor.BestKnownImplementationType)) {
                registration.Activated += OnActivated;
            }
        }

        static void OnActivated(object sender, Autofac.ActivatedEventArgs e) {
            var userContainer = (ICurrentUser)e.Instance;
            var authenticationService = e.Context.Resolve<IAuthenticationService>();
            userContainer.CurrentUser = authenticationService.Authenticated();
        }
    }
}
