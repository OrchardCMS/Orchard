using System;
using System.Threading;
using System.Web;
using Autofac;
using Orchard.Environment;

namespace Orchard.WarmupStarter {
    public class Starter {
        private static IOrchardHost _host;
        private static Exception _error;

        public static void OnBeginRequest(HttpContext context, Action<ContainerBuilder> registrations) {
            if (_error != null) {
                // Host startup resulted in an error

                // Throw error once, and restart launch; machine state may have changed
                // so we need to simulate a "restart".
                var error = _error;
                LaunchStartupThread(registrations);
                throw new ApplicationException("Error during Orchard startup", error);
            }

            // Only notify if the host has started up
            if (_host == null) {
                return;
            }

            context.Items["originalHttpContext"] = context;
            _host.BeginRequest();   
        }

        public static void OnEndRequest() {
            // Only notify if the host has started up
            if (_host != null) {
                _host.EndRequest();
            }            
        }

        /// <summary>
        /// Initializes Orchard's Host in a separate thread
        /// </summary>
        public static void LaunchStartupThread(Action<ContainerBuilder> registrations) {
            _host = null;
            _error = null;

            ThreadPool.QueueUserWorkItem(
                state => {
                    try {
                        var host = OrchardStarter.CreateHost(registrations);
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

                    // initialize shells to speed up the first dynamic query
                    if (_host != null) {
                        _host.BeginRequest();
                        _host.EndRequest();
                    }
                });
        }
    }
}
