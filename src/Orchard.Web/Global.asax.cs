using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;
using Orchard.Environment.Warmup;
using Orchard.Utility.Extensions;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static StartupResult _startupResult;
        private static EventWaitHandle _waitHandle;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            LaunchStartupThread();
        }

        /// <summary>
        /// Initializes Orchard's Host in a separate thread
        /// </summary>
        private static void LaunchStartupThread() {
            _startupResult = new StartupResult();
            _waitHandle = new AutoResetEvent(false);

            ThreadPool.QueueUserWorkItem(
                state => {
                    try {
                        RegisterRoutes(RouteTable.Routes);
                        var host = OrchardStarter.CreateHost(MvcSingletons);
                        host.Initialize();
                        _startupResult.Host = host;
                    }
                    catch (Exception e) {
                        _startupResult.Error = e;
                    }
                    finally {
                        _waitHandle.Set();
                    }
                });
        }

        protected void Application_BeginRequest() {
            // Host is still starting up?
            if (_startupResult.Host == null && _startupResult.Error == null) {

                // use the url as it was requested by the client
                // the real url might be different if it has been translated (proxy, load balancing, ...)
                var url = Request.ToUrlString();
                var virtualFileCopy = "~/App_Data/WarmUp/" + WarmupUtility.EncodeUrl(url.Trim('/'));
                var localCopy = HostingEnvironment.MapPath(virtualFileCopy);

                if (File.Exists(localCopy)) {
                    // result should not be cached, even on proxies
                    Context.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                    Context.Response.Cache.SetValidUntilExpires(false);
                    Context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                    Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Context.Response.Cache.SetNoStore();

                    Context.Response.WriteFile(localCopy);
                    Context.Response.End();
                }
                else if(!File.Exists(Request.PhysicalPath)) {
                    // there is no local copy and the host is not running
                    // wait for the host to initialize
                    _waitHandle.WaitOne();
                }
            }
            else {
                if (_startupResult.Error != null) {
                    // Host startup resulted in an error

                    // Throw error once, and restart launch (machine state may have changed
                    // so we need to simulate a "restart".
                    var error = _startupResult.Error;
                    LaunchStartupThread();
                    throw error;
                }

                Context.Items["originalHttpContext"] = Context;
                _startupResult.Host.BeginRequest();
            }
        }

        protected void Application_EndRequest() {
            // Only notify if the host has started up
            if (_startupResult.Host != null) {
                _startupResult.Host.EndRequest();
            }
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }
    }
}
