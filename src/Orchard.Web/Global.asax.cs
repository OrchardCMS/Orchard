using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static IOrchardHost _host;
        private static Exception _error;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            LaunchStartupThread();
        }

        protected void Application_BeginRequest() {
            if (_error != null) {
                // Host startup resulted in an error

                // Throw error once, and restart launch; machine state may have changed
                // so we need to simulate a "restart".
                var error = _error;
                LaunchStartupThread();
                throw error;
            }

            // Only notify if the host has started up
            if (_host == null) {
                return;
            }

            Context.Items["originalHttpContext"] = Context;
            _host.BeginRequest();
        }

        protected void Application_EndRequest() {
            // Only notify if the host has started up
            if (_host != null) {
                _host.EndRequest();
            }
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }

        /// <summary>
        /// Initializes Orchard's Host in a separate thread
        /// </summary>
        private static void LaunchStartupThread() {
            RegisterRoutes(RouteTable.Routes);

            _host = null;
            _error = null;

            ThreadPool.QueueUserWorkItem(
                state => {
                    try {
                        var host = OrchardStarter.CreateHost(MvcSingletons);
                        host.Initialize();
                        _host = host;
                    }
                    catch (Exception e) {
                        _error = e;
                    }
                    finally {
                        // Execute pending actions as the host is available
                        WarmupHttpModule.Signal();
                    }
                });
        }
    }
}
