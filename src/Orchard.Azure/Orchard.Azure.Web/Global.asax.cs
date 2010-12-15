using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.Environment;

namespace Orchard.Azure.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static IOrchardHost _host;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            CloudStorageAccount.SetConfigurationSettingPublisher(    
                (configName, configSetter) =>    
                    configSetter(RoleEnvironment.GetConfigurationSettingValue(configName))    
                );

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += (sender, e) => {
                                            // If a configuration setting is changing
                                            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange)) {
                                                // Set e.Cancel to true to restart this role instance
                                                e.Cancel = true;
                                            }
                                        };

            RegisterRoutes(RouteTable.Routes);

            _host = OrchardStarter.CreateHost(MvcSingletons);
            _host.Initialize();
        }

        protected void Application_BeginRequest() {
            Context.Items["originalHttpContext"] = Context;

            _host.BeginRequest();
        }

        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }

    }
}
