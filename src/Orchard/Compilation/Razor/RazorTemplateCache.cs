using System.Collections.Concurrent;

namespace Orchard.Compilation.Razor {
    public class RazorTemplateHolder : IRazorTemplateHolder {
        private readonly ConcurrentDictionary<string, string> _cache; 

        public RazorTemplateHolder() {
            _cache = new ConcurrentDictionary<string, string>();
        }
        public string Get(string name) {
            string template;
            return _cache.TryGetValue(name, out template) ? template : null;
        }

        public void Set(string name, string template) {
            _cache.AddOrUpdate(name, template, (s, s1) => template);
        }
    }
}