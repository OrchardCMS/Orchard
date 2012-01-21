using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement {
    public class FuncDictionary<TKey, TVal> {

        private readonly IDictionary<TKey, TVal> _cached;
        private readonly IDictionary<TKey, Func<TVal>> _factories;

        public FuncDictionary() {
            _cached = new Dictionary<TKey, TVal>();
            _factories = new Dictionary<TKey, Func<TVal>>();
        }

        public TVal Get(TKey key) {
            if (!_cached.ContainsKey(key)) {
                if (!_factories.ContainsKey(key)) return default(TVal);
                _cached[key] = _factories[key]();
            }
            return _cached[key];
        }

        public void Set(TKey key, Func<TVal> factory) {
            _cached.Remove(key);
            _factories[key] = factory;
        }

        public void Remove(TKey key) {
            _cached.Remove(key);
            _factories.Remove(key);
        }

    }
}
