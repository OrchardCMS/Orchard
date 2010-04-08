using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public void RunSingleCommand(string[] args) {
            try {
                var context = ParseArguments(args);

                var hostContainer = OrchardStarter.CreateHostContainer(MvcSingletons);
                var host = hostContainer.Resolve<IOrchardHost>();
                var tenantManager = hostContainer.Resolve<ITenantManager>();

                host.Initialize();

                // Find the shell settings for the tenant...

                // cretae the stand-alone env

                // resolve a command
            }
            catch (Exception e) {
                for(; e != null; e = e.InnerException) {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }
        }

        private static CommandContext ParseArguments(IEnumerable<string> args) {
            var arguments = new List<string>();
            var switches = new NameValueCollection();

            foreach (string arg in args) {
                if (arg[0] == '/') {
                    string[] split = arg.Substring(1).Split(':');
                    switches.Add(split[0], split.Length >= 2 ? split[1] : string.Empty);
                }
                else {
                    arguments.Add(arg);
                }
            }

            return new CommandContext {
                Input = "",
                Output = "",
                Command = arguments[0],
                Arguments = arguments.Skip(1).ToArray(),
                Switches = switches
            };
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
