namespace Orchard.Environment.Compilation.Compilers {
    public interface IExtensionCompiler {
        void Compile(CompileExtensionContext context);
    }
}
