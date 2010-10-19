using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.State;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks;

namespace Orchard.Commands {
    /// <summary>
    /// This is the guy instantiated by the orchard.exe host. It is reponsible for
    /// executing a single command.
    /// </summary>
    public class CommandHostAgent {
        private IContainer _hostContainer;

        public CommandHostAgent() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }


        public int RunSingleCommand(TextReader input, TextWriter output, string tenant, string[] args, Dictionary<string, string> switches) {
            int result = StartHost(input, output);
            if (result != 0)
                return result;

            result = RunCommand(input, output, tenant, args, switches);
            if (result != 0)
                return result;

            return StopHost(input, output);
        }

        public int RunCommand(TextReader input, TextWriter output, string tenant, string[] args, Dictionary<string, string> switches) {
            try {
                tenant = tenant ?? "Default";

                using (var env = CreateStandaloneEnvironment(tenant)) {
                    var commandManager = env.Resolve<ICommandManager>();
                    var transactionManager = env.Resolve<ITransactionManager>();

                    var parameters = new CommandParameters {
                        Arguments = args,
                        Switches = switches,
                        Input = input,
                        Output = output
                    };

                    try {
                        commandManager.Execute(parameters);
                    }
                    catch {
                        // any database changes in this using(env) scope are invalidated
                        transactionManager.Cancel();

                        // exception handling performed below
                        throw;
                    }
                }

                // in effect "pump messages" see PostMessage circa 1980
                var processingEngine = _hostContainer.Resolve<IProcessingEngine>();
                while (processingEngine.AreTasksPending())
                    processingEngine.ExecuteNextTask();

                return 0;
            }
            catch (OrchardCommandHostRetryException e) {
                // Special "Retry" return code for our host
                output.WriteLine("{0} (Retrying...)", e.Message);
                return 240;
            }
            catch (Exception e) {
                for (int i = 0; e != null; e = e.InnerException, i++) {
                    if (i > 0) {
                        output.WriteLine("-------------------------------------------------------------------");
                    }
                    output.WriteLine("Error: {0}", e.Message);
                    output.WriteLine("{0}", e.StackTrace);
                }
                return 5;
            }
        }

        public int StartHost(TextReader input, TextWriter output) {
            try {
                _hostContainer = CreateHostContainer();
                return 0;
            }
            catch (OrchardCommandHostRetryException e) {
                // Special "Retry" return code for our host
                output.WriteLine("{0} (Retrying...)", e.Message);
                return 240;
            }
            catch (Exception e) {
                for (; e != null; e = e.InnerException) {
                    output.WriteLine("Error: {0}", e.Message);
                    output.WriteLine("{0}", e.StackTrace);
                }
                return 5;
            }
        }

        public int StopHost(TextReader input, TextWriter output) {
            try {
                if (_hostContainer != null) {
                    _hostContainer.Dispose();
                    _hostContainer = null;
                }
                return 0;
            }
            catch (Exception e) {
                for (; e != null; e = e.InnerException) {
                    output.WriteLine("Error: {0}", e.Message);
                    output.WriteLine("{0}", e.StackTrace);
                }
                return 5;
            }
        }

        private IContainer CreateHostContainer() {
            var hostContainer = OrchardStarter.CreateHostContainer(ContainerRegistrations);

            var host = hostContainer.Resolve<IOrchardHost>();
            host.Initialize();
            return hostContainer;
        }


        private IWorkContextScope CreateStandaloneEnvironment(string tenant) {
            var host = _hostContainer.Resolve<IOrchardHost>();
            var tenantManager = _hostContainer.Resolve<IShellSettingsManager>();

            // Retrieve settings for speficified tenant.
            var settingsList = tenantManager.LoadSettings();
            if (settingsList.Any()) {
                var settings = settingsList.SingleOrDefault(s => String.Equals(s.Name, tenant, StringComparison.OrdinalIgnoreCase));
                if (settings == null) {
                    throw new OrchardCoreException(T("Tenant {0} does not exist", tenant));
                }

                var env = host.CreateStandaloneEnvironment(settings);
                return env;
            }
            else {
                // In case of an unitiliazed site (no default settings yet), we create a default settings instance.
                var settings = new ShellSettings { Name = "Default", State = new TenantState("Uninitialized") };
                return host.CreateStandaloneEnvironment(settings);
            }
        }


        protected void ContainerRegistrations(ContainerBuilder builder) {
            MvcSingletons(builder);

            builder.RegisterType<CommandHostEnvironment>().As<IHostEnvironment>().SingleInstance();
            builder.RegisterType<CommandHostVirtualPathMonitor>().As<IVirtualPathMonitor>().As<IVolatileProvider>().SingleInstance();
            builder.RegisterInstance(CreateShellRegistrations()).As<IShellContainerRegistrations>();
        }

        private CommandHostShellContainerRegistrations CreateShellRegistrations() {
            return new CommandHostShellContainerRegistrations {
                Registrations = shellBuilder => {
                                    shellBuilder.RegisterType<CommandHostVirtualPathMonitor>()
                                        .As<IVirtualPathMonitor>()
                                        .As<IVolatileProvider>()
                                        .InstancePerMatchingLifetimeScope("shell");
                                    shellBuilder.RegisterType<CommandBackgroundService>()
                                        .As<IBackgroundService>()
                                        .InstancePerLifetimeScope();
                                }
            };
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }

        private class CommandHostShellContainerRegistrations : IShellContainerRegistrations {
            public Action<ContainerBuilder> Registrations { get; set; }
        }
    }
}
