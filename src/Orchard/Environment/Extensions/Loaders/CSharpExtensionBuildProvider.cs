using System.Web.Compilation;

namespace Orchard.Environment.Extensions.Loaders {
    public class CSharpExtensionBuildProvider : BuildProvider {
        private readonly CompilerType _codeCompilerType;

        public CSharpExtensionBuildProvider() {
            _codeCompilerType = GetDefaultCompilerTypeForLanguage("C#");
        }

        public override CompilerType CodeCompilerType { get { return _codeCompilerType; } }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
            var virtualPathProvider = new AspNetVirtualPathProvider();
            var compiler = new CSharpProjectMediumTrustCompiler(virtualPathProvider);

            var aspNetAssemblyBuilder = new AspNetAssemblyBuilder(assemblyBuilder, this);
            compiler.CompileProject(this.VirtualPath, aspNetAssemblyBuilder);
        }
    }
}