namespace Orchard.Compilation.Razor {
    public interface IRazorTemplateCache {
        string Get(string name);
        void Set(string name, string template);
    }
}