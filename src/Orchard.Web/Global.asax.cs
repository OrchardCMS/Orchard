using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Builder;
using Orchard.Environment;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static IOrchardHost _host;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = "" } // Parameter defaults
                );
        }

        static IEnumerable<string> OrchardLocationFormats() {
            return new[] {
                "~/Packages/{2}/Views/{1}/{0}.aspx",
                "~/Packages/{2}/Views/{1}/{0}.ascx",
                "~/Packages/{2}/Views/Shared/{0}.aspx",
                "~/Packages/{2}/Views/Shared/{0}.ascx",
                "~/Core/{2}/Views/{1}/{0}.aspx",
                "~/Core/{2}/Views/{1}/{0}.ascx",
                "~/Core/{2}/Views/Shared/{0}.aspx",
                "~/Core/{2}/Views/Shared/{0}.ascx",
            };
        }

        protected void Application_Start() {
            // This is temporary until MVC2 is officially released.
            // We want to avoid running against an outdated preview installed in the GAC
            CheckMvcVersion(new Version("2.0.41116.0")/*MVC2 Beta file version #*/);
            RegisterRoutes(RouteTable.Routes);

            _host = OrchardStarter.CreateHost(MvcSingletons);
            _host.Initialize();

            //TODO: what's the failed initialization story - IoC failure in app start can leave you with a zombie appdomain
        }

        private void CheckMvcVersion(Version requiredVersion) {
            Assembly loadedMvcAssembly = typeof(System.Web.Mvc.Controller).Assembly;
            Version loadedMvcVersion = ReadAssemblyFileVersion(loadedMvcAssembly);

            if (loadedMvcVersion != requiredVersion) {
                string message;
                if (loadedMvcAssembly.GlobalAssemblyCache) {
                    message = string.Format(
                        "Orchard has been deployed with a version of {0} that has a different file version ({1}) " +
                        "than the version installed in the GAC ({2}).\r\n" +
                        "This implies that Orchard will not be able to run properly in this machine configuration.\r\n" +
                        "Please un-install MVC from the GAC or install a more recent version.",
                        loadedMvcAssembly.GetName().Name,
                        loadedMvcVersion,
                        requiredVersion);
                }
                else {
                    message = string.Format(
                        "Orchard has been configured to run with a file version {1} of \"{0}\" " +
                        "but the version deployed with the application is {2}.\r\n" +
                        "This probably implies that Orchard is deployed with a newer version " +
                        "and the source code hasn't been updated accordingly.\r\n" +
                        "Update the Orchard.Web application source code (look for \"CheckMvcVersion\") to " + 
                        "specify the correct file version number.\r\n",
                        loadedMvcAssembly.GetName().Name,
                        loadedMvcVersion,
                        requiredVersion);
                }

                throw new HttpException(500, message);
            }
        }

        private Version ReadAssemblyFileVersion(Assembly assembly) {
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
            if (attributes == null || attributes.Length != 1) {
                string message = string.Format("Assembly \"{0}\" doesn't have a \"{1}\" attribute",
                    assembly.GetName().Name, typeof(AssemblyFileVersionAttribute).FullName);
                throw new FileLoadException(message);
            }

            var attribute = (AssemblyFileVersionAttribute)attributes[0];
            return new Version(attribute.Version);
        }

        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        protected void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ControllerBuilder.Current);
            builder.Register(RouteTable.Routes);
            builder.Register(ModelBinders.Binders);
            builder.Register(ModelMetadataProviders.Current);
            builder.Register(ViewEngines.Engines);
        }
    }
}
