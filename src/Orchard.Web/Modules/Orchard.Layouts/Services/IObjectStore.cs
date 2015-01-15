using System;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    /// <summary>
    /// A simple object store to store temporary data into. It is used to transfer layout element state between the canvas and the element editor.
    /// </summary>
    public interface IObjectStore : IDependency {
        ObjectStoreEntry GetEntry(string key);
        ObjectStoreEntry GetOrCreateEntry(string key);
        ObjectStoreEntry GetOrCreateEntry();
        void Set<T>(string key, T value);
        string Set<T>(T value);
        T Get<T>(string key, Func<T> defaultValue = null);
        string GenerateKey();
        bool Remove(string key);
    }
}