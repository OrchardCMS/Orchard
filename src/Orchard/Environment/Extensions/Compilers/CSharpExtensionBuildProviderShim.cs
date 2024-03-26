using System.Reflection;
using System.Web.Compilation;

namespace Orchard.Environment.Extensions.Compilers {
    public class CSharpExtensionBuildProviderShim : BuildProvider, IShim {
        private readonly CompilerType _codeCompilerType;

        public CSharpExtensionBuildProviderShim() {
            OrchardHostContainerRegistry.RegisterShim(this);

            _codeCompilerType = GetDefaultCompilerTypeForLanguage("C#");

            var orchardVersion = new AssemblyName(typeof(IDependency).Assembly.FullName).Version;
            // Additional options after the ones defined in web.config require to be separated by a leading space character.
            _codeCompilerType.CompilerParameters.CompilerOptions += $" /define:ORCHARD_{orchardVersion.Major}_{orchardVersion.Minor}";
        }

        public IOrchardHostContainer HostContainer { get; set; }

        public override CompilerType CodeCompilerType {
            get {
                return _codeCompilerType;
            }
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
            var context = new CompileExtensionContext {
                VirtualPath = this.VirtualPath,
                AssemblyBuilder = new AspNetAssemblyBuilder(assemblyBuilder, this)
            };
            HostContainer.Resolve<IExtensionCompiler>().Compile(context);
        }
    }
}