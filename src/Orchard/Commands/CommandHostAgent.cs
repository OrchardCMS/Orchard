using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Orchard.Commands {
    /// <summary>
    /// This is the guy instantiated by the orchard.exe host. It is reponsible for
    /// executing a single command.
    /// </summary>
    public class CommandHostAgent {
        public int RunSingleCommand(TextReader input, TextWriter output, string tenant, string[] args, Dictionary<string,string> switches) {
            try {
                var hostContainer = OrchardStarter.CreateHostContainer(MvcSingletons);
                var host = hostContainer.Resolve<IOrchardHost>();
                host.Initialize();

                // Find tenant (or default)
                tenant = tenant ?? "default";
                var tenantManager = hostContainer.Resolve<IShellSettingsManager>();
                var tenantSettings = tenantManager.LoadSettings().Single(s => String.Equals(s.Name, tenant, StringComparison.OrdinalIgnoreCase));

                // Disptach command execution to ICommandManager
                using (var env = host.CreateStandaloneEnvironment(tenantSettings)) {
                    var parameters = new CommandParameters {
                        Arguments = args, 
                        Switches = switches,
                        Input = input,
                        Output = output
                    };
                    env.Resolve<ICommandManager>().Execute(parameters);
                }

                return 0;
            }
            catch (Exception e) {
                for(; e != null; e = e.InnerException) {
                    output.WriteLine("Error: {0}", e.Message);
                    output.WriteLine("{0}", e.StackTrace);
                }
                return 5;
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
