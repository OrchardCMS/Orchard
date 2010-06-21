using System.CodeDom;
using System.IO;
using System.Linq;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.Environment.Extensions.Compilers {
    /// <summary>
    /// Compile a C# extension into an assembly given a directory location
    /// </summary>
    public class DefaultExtensionCompiler : IExtensionCompiler {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IProjectFileParser _projectFileParser;

        public DefaultExtensionCompiler(IVirtualPathProvider virtualPathProvider, IProjectFileParser projectFileParser) {
            _virtualPathProvider = virtualPathProvider;
            _projectFileParser = projectFileParser;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Compile(CompileExtensionContext context) {
            Logger.Information("Generate code for file \"{0}\"", context.VirtualPath);

            using (var stream = _virtualPathProvider.OpenFile(context.VirtualPath)) {
                var descriptor = _projectFileParser.Parse(stream);

                var directory = _virtualPathProvider.GetDirectoryName(context.VirtualPath);
                foreach (var filename in descriptor.SourceFilenames.Select(f => _virtualPathProvider.Combine(directory, f))) {
                    context.AssemblyBuilder.AddCodeCompileUnit(CreateCompileUnit(filename));
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