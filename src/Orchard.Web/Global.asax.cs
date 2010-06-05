using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;
using Orchard.Environment.Extensions.Loaders;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static IOrchardHost _host;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            // This is temporary until MVC2 is officially released.
            // We want to avoid running against an outdated preview installed in the GAC
            CheckMvcVersion(
                new Version("2.0.50217.0")/*MVC2 RTM file version #*/,
                new Version("2.0.50129.0")/*MVC2 RC2 file version #*/,
                new Version("2.0.41211.0")/*MVC2 RC file version #*/);

            RegisterRoutes(RouteTable.Routes);

            HostingEnvironment.RegisterVirtualPathProvider(new WebFormsExtensionsVirtualPathProvider());
            _host = OrchardStarter.CreateHost(MvcSingletons);
            _host.Initialize();

            //TODO: what's the failed initialization story - IoC failure in app start can leave you with a zombie appdomain
        }

        protected void Application_BeginRequest() {
            Context.Items["originalHttpContext"] = Context;

            _host.BeginRequest();
        }

        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        private void CheckMvcVersion(params Version[] allowedVersions) {
            Assembly loadedMvcAssembly = typeof(System.Web.Mvc.Controller).Assembly;
            Version loadedMvcVersion = ReadAssemblyFileVersion(loadedMvcAssembly);

            if (allowedVersions.All(allowed => loadedMvcVersion != allowed)) {
                string message;
                if (loadedMvcAssembly.GlobalAssemblyCache) {
                    message = string.Format(
                        "Orchard has been deployed with a version of {0} that has a different file version ({1}) " +
                        "than the version installed in the GAC ({2}).\r\n" +
                        "This implies that Orchard will not be able to run properly in this machine configuration.\r\n" +
                        "Please un-install MVC from the GAC or install a more recent version.",
                        loadedMvcAssembly.GetName().Name,
                        allowedVersions.First(),
                        loadedMvcVersion);
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
                        allowedVersions.First(),
                        loadedMvcVersion);
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


        protected void MvcSingletons(ContainerBuilder builder) {
            builder.RegisterInstance(ControllerBuilder.Current);
            builder.RegisterInstance(RouteTable.Routes);
            builder.RegisterInstance(ModelBinders.Binders);
            builder.RegisterInstance(ModelMetadataProviders.Current);
            builder.RegisterInstance(ViewEngines.Engines);
        }
    }
}
