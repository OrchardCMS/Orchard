namespace Orchard.Compilation.Razor {
    public interface IRazorTemplateHolder : ISingletonDependency {
        string Get(string name);
        void Set(string name, string template);
    }
}