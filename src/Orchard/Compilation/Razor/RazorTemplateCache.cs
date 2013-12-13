using System.Collections.Concurrent;

namespace Orchard.Compilation.Razor {
    public class RazorTemplateCache : IRazorTemplateCache {
        private readonly ConcurrentDictionary<string, string> _cache; 

        public RazorTemplateCache() {
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