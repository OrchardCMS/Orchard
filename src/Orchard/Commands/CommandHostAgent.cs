using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Commands {
    /// <summary>
    /// This is the guy instantiated by the orchard.exe host. It is reponsible for
    /// executing a single command.
    /// </summary>
    public class CommandHostAgent {
        private IContainer _hostContainer;
        private Dictionary<string, IStandaloneEnvironment> _tenants;

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

        public int StartHost(TextReader input, TextWriter output) {
            try {
                _hostContainer = OrchardStarter.CreateHostContainer(MvcSingletons);
                _tenants = new Dictionary<string, IStandaloneEnvironment>();

                var host = _hostContainer.Resolve<IOrchardHost>();
                host.Initialize();
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


        public int StopHost(TextReader input, TextWriter output) {
            try {
                foreach (var tenant in _tenants.Values) {
                    tenant.Dispose();
                }
                _hostContainer.Dispose();

                _tenants = null;
                _hostContainer = null;
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

        public int RunCommand(TextReader input, TextWriter output, string tenant, string[] args, Dictionary<string, string> switches) {
            try {
                tenant = tenant ?? "Default";

                var env = FindOrCreateTenant(tenant);

                var parameters = new CommandParameters {
                    Arguments = args,
                    Switches = switches,
                    Input = input,
                    Output = output
                };

                env.Resolve<ICommandManager>().Execute(parameters);

                return 0;
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

        public IStandaloneEnvironment FindOrCreateTenant(string tenant) {
            IStandaloneEnvironment result;
            if (_tenants.TryGetValue(tenant, out result))
                return result;

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

                // Store in cache for next calls
                _tenants.Add(tenant, env);
                return env;
            }
            else {
                // In case of an unitiliazed site (no default settings yet), we create a default settings instance.
                var settings = new ShellSettings {Name = "Default", State = new TenantState("Uninitialized")};
                return host.CreateStandaloneEnvironment(settings);
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
