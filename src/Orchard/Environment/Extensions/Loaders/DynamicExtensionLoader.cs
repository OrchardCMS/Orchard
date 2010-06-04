using System;
using System.IO;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    public class DynamicExtensionLoader : IExtensionLoader {
        public int Order { get { return 10; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            // 1) Try to load the assembly directory
            //    This will look in the "App_Data/Dependencies" directory if
            //    the probing path is correctly configured in Web.config
            {
                try {
                    Assembly assembly = Assembly.Load(descriptor.Name);
                    return CreateExtensionEntry(descriptor, assembly);
                }
                catch (FileNotFoundException e) {
                    // The assembly is not in one of the probing directory, 
                    // including "App_Data/Dependencies", we need to move on
                    // to other strageties
                }
            }

            // 2) look for the assembly in the "Bin" directory
            {
                string modulePath = HostingEnvironment.MapPath(descriptor.Location);
                modulePath = Path.Combine(modulePath, descriptor.Name);
                string moduleBinary = Path.Combine(modulePath, "bin");
                moduleBinary = Path.Combine(moduleBinary, descriptor.Name + ".dll");
                if (File.Exists(moduleBinary)) {
                    // Copy file to dependencies directory
                    string dependenciesPath = HostingEnvironment.MapPath("~/App_Data/Dependencies");
                    if (!Directory.Exists(dependenciesPath)) {
                        Directory.CreateDirectory(dependenciesPath);
                    }
                    string destFile = Path.Combine(dependenciesPath, descriptor.Name + ".dll");
                    File.Copy(moduleBinary, destFile, true);

                    // then load the assembly
                    Assembly assembly = Assembly.Load(descriptor.Name);
                    return CreateExtensionEntry(descriptor, assembly);
                }
            }

            // 3) look for the csproj in the module directory
            {
                string projfileName = Path.Combine(descriptor.Location, descriptor.Name);
                projfileName = Path.Combine(projfileName, descriptor.Name + ".csproj").Replace('\\', '/');
                var assembly = BuildManager.GetCompiledAssembly(projfileName);
                return CreateExtensionEntry(descriptor, assembly);
            }
        }

        private static ExtensionEntry CreateExtensionEntry(ExtensionDescriptor descriptor, Assembly assembly) {
            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes(),
            };
        }
    }
}