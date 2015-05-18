using System;

namespace Orchard.Layouts.Services {
    /// <summary>
    /// A simple object store to store temporary data. It is used to transfer layout element data between the canvas and the element editor.
    /// </summary>
    public interface IObjectStore : IDependency {
        void Set(string key, object value);
        object Get(string key, Func<object> defaultValue = null);
        string GenerateKey();
        void Remove(string key);
    }
}