using System.Web.Compilation;

namespace Orchard.Environment.Extensions.Compilers {
    public class CSharpExtensionBuildProvider : BuildProvider {
        private readonly CompilerType _codeCompilerType;

        public CSharpExtensionBuildProvider() {
            _codeCompilerType = GetDefaultCompilerTypeForLanguage("C#");
        }

        public override CompilerType CodeCompilerType { get { return _codeCompilerType; } }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
            var virtualPathProvider = new DefaultVirtualPathProvider();
            var compiler = new CSharpProjectMediumTrustCompiler(virtualPathProvider);

            var aspNetAssemblyBuilder = new AspNetAssemblyBuilder(assemblyBuilder, this);
            compiler.CompileProject(this.VirtualPath, aspNetAssemblyBuilder);
        }
    }
}