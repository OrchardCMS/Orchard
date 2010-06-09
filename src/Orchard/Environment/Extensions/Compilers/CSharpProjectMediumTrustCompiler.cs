using System.CodeDom;
using System.IO;
using System.Linq;

namespace Orchard.Environment.Extensions.Compilers {
    /// <summary>
    /// Compile a C# extension into an assembly given a directory location
    /// </summary>
    public class CSharpProjectMediumTrustCompiler {
        private readonly IVirtualPathProvider _virtualPathProvider;

        public CSharpProjectMediumTrustCompiler(IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;
        }
        /// <summary>
        /// Compile a csproj file given its virtual path, a build provider and an assembly builder.
        /// This method works in medium trust.
        /// </summary>
        public void CompileProject(string virtualPath, IAssemblyBuilder assemblyBuilder) {
            using (var stream = _virtualPathProvider.OpenFile(virtualPath)) {
                var descriptor = new CSharpProjectParser().Parse(stream);

                var directory = _virtualPathProvider.GetDirectoryName(virtualPath);
                foreach (var filename in descriptor.SourceFilenames.Select(f => _virtualPathProvider.Combine(directory, f))) {
                    assemblyBuilder.AddCodeCompileUnit(CreateCompileUnit(filename));
                }
            }
        }

        private CodeCompileUnit CreateCompileUnit(string virtualPath) {
            var contents = GetContents(virtualPath);
            var unit = new CodeSnippetCompileUnit(contents);
            var physicalPath = _virtualPathProvider.MapPath(virtualPath);
            if (!string.IsNullOrEmpty(physicalPath)) {
                unit.LinePragma = new CodeLinePragma(physicalPath, 1);
            }
            return unit;
        }

        private string GetContents(string virtualPath) {
            string contents;
            using (var stream = _virtualPathProvider.OpenFile(virtualPath)) {
                using (var reader = new StreamReader(stream)) {
                    contents = reader.ReadToEnd();
                }
            }
            return contents;
        }
    }
}