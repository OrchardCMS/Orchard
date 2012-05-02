using System.Reflection;
using System.Web.Compilation;

namespace Orchard.Environment.Extensions.Compilers {
    public class CSharpExtensionBuildProviderShim : BuildProvider, IShim {
        private readonly CompilerType _codeCompilerType;

        public CSharpExtensionBuildProviderShim() {
            OrchardHostContainerRegistry.RegisterShim(this);

            _codeCompilerType = GetDefaultCompilerTypeForLanguage("C#");

            // NOTE: This code could be used to define a compilation flag with the current Orchar version 
            // but it's not compatible with Medium Trust
            // var orchardVersion = new AssemblyName(typeof(IDependency).Assembly.FullName).Version;
            // _codeCompilerType.CompilerParameters.CompilerOptions += string.Format("/define:ORCHARD_{0}_{1}", orchardVersion.Major, orchardVersion.Minor);
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
                AssemblyBuilder =  new AspNetAssemblyBuilder(assemblyBuilder, this)
            };
            HostContainer.Resolve<IExtensionCompiler>().Compile(context);
        }
    }
}