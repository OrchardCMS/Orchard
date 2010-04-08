using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;

namespace Orchard.Commands {
    /// <summary>
    /// This is the guy instantiated by the orchard.exe host. It is reponsible for
    /// executing a single command.
    /// </summary>
    public class CommandAgent {
        public void RunSingleCommand(string[] args) {
            try {
                Console.WriteLine("Command being run!");

                var host = OrchardStarter.CreateHost(MvcSingletons);
                host.Initialize();

                var shell = host.
            }
            catch (Exception e) {
                while (e != null) {
                    Console.WriteLine("Error: {0}", e.Message);
                    e = e.InnerException;
                }
            }
        }

        protected void MvcSingletons(ContainerBuilder builder) {
            builder.RegisterInstance(ControllerBuilder.Current);
            builder.RegisterInstance(RouteTable.Routes);
            builder.RegisterInstance(ModelBinders.Binders);
            builder.RegisterInstance(ModelMetadataProviders.Current);
            builder.RegisterInstance(ViewEngines.Engines);
        }
    }
}
