using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;

namespace Orchard.Extensions.Loaders {
    public class DynamicExtensionLoader : IExtensionLoader {
        public int Order { get { return 10; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            if (HostingEnvironment.IsHosted == false)
                return null;

            var codeProvider = CodeDomProvider.CreateProvider("cs");

            var references = GetAssemblyReferenceNames();
            var options = new CompilerParameters(references.ToArray());

            var locationPath = HostingEnvironment.MapPath(descriptor.Location);
            var extensionPath = Path.Combine(locationPath, descriptor.Name);

            var fileNames = GetSourceFileNames(extensionPath);
            var results = codeProvider.CompileAssemblyFromFile(options, fileNames.ToArray());

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = results.CompiledAssembly,
                ExportedTypes = results.CompiledAssembly.GetExportedTypes(),
            };
        }

        private IEnumerable<string> GetAssemblyReferenceNames() {
            return BuildManager.GetReferencedAssemblies()
                .OfType<Assembly>()
                .Select(x => x.Location)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();
        }

        private IEnumerable<string> GetSourceFileNames(string path) {
            foreach (var file in Directory.GetFiles(path, "*.cs")) {
                yield return file;
            }

            foreach (var folder in Directory.GetDirectories(path)) {
                if (Path.GetFileName(folder).StartsWith("."))
                    continue;

                foreach (var file in GetSourceFileNames(folder)) {
                    yield return file;
                }
            }
        }
    }
}
