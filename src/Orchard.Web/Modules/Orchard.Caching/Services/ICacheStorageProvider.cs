using System;

namespace Orchard.Caching.Services {
    public interface ICacheStorageProvider : IDependency {
        object Get(string key);
        void Put(string key, object value);
        void Put(string key, object value, TimeSpan validFor);
        void Remove(string key);
        void Clear();
    }
}