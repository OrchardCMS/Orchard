using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment.Extensions.Compilers {
    /// <summary>
    /// Compile a C# extension into an assembly given a directory location
    /// Note: currently not used...
    /// </summary>
    public class CSharpProjectFullTrustCompiler {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IBuildManager _buildManager;

        public CSharpProjectFullTrustCompiler(IVirtualPathProvider virtualPathProvider, IBuildManager buildManager) {
            _virtualPathProvider = virtualPathProvider;
            _buildManager = buildManager;
        }

        /// <summary>
        /// Compile a csproj file given its virtual path. Use the CSharp CodeDomProvider
        /// class, which is only available in full trust.
        /// </summary>
        public CompilerResults CompileProject(string virtualPath, string outputDirectory) {
            var codeProvider = CodeDomProvider.CreateProvider("cs");
            var directory = _virtualPathProvider.GetDirectoryName(virtualPath);

            using (var stream = _virtualPathProvider.OpenFile(virtualPath)) {
                var descriptor = new DefaultProjectFileParser().Parse(stream);

                var references = GetReferencedAssembliesLocation();
                var options = new CompilerParameters(references.ToArray());
                options.GenerateExecutable = false;
                options.OutputAssembly = Path.Combine(outputDirectory, descriptor.AssemblyName + ".dll");

                var fileNames = descriptor.SourceFilenames
                    .Select(f => _virtualPathProvider.Combine(directory, f))
                    .Select(f => _virtualPathProvider.MapPath(f));

                var results = codeProvider.CompileAssemblyFromFile(options, fileNames.ToArray());
                return results;
            }
        }

        private IEnumerable<string> GetReferencedAssembliesLocation() {
            return _buildManager.GetReferencedAssemblies()
                .Select(a => a.Location)
                .Where(a => !string.IsNullOrEmpty(a))
                .Distinct();
        }
    }
}