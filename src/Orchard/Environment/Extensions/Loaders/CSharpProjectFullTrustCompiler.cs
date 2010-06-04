using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Compile a C# extension into an assembly given a directory location
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
        public CompilerResults CompileProject(string virtualPath) {
            var codeProvider = CodeDomProvider.CreateProvider("cs");
            var directory = _virtualPathProvider.GetDirectoryName(virtualPath);

            using (var stream = _virtualPathProvider.OpenFile(virtualPath)) {
                var descriptor = new CSharpProjectParser().Parse(stream);

                var references = GetAssemblyReferenceNames();
                var options = new CompilerParameters(references.ToArray());

                var fileNames = descriptor.SourceFilenames
                    .Select(f => _virtualPathProvider.Combine(directory, f))
                    .Select(f => _virtualPathProvider.MapPath(f));

                var results = codeProvider.CompileAssemblyFromFile(options, fileNames.ToArray());
                return results;
            }
        }

        private IEnumerable<string> GetAssemblyReferenceNames() {
            return _buildManager.GetReferencedAssemblies()
                .OfType<Assembly>()
                .Where(a => !string.IsNullOrEmpty(a.Location))
                .Select(a => a.Location)
                .Distinct();
        }
    }
}