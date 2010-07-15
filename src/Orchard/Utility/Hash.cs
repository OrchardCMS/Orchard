using System;

namespace Orchard.Utility {
    /// <summary>
    /// Compute an (almost) unique hash value from various sources.
    /// This allows computing hash keys that are easily storable
    /// and comparable from heterogenous components.
    /// </summary>
    public class Hash {
        private long _hash;

        public string Value { get { return _hash.ToString(); } }

        public void AddString(string value) {
            _hash += value.GetHashCode();
        }

        public void AddTypeReference(Type type) {
            AddString(type.AssemblyQualifiedName);
            AddString(type.FullName);
        }
    }
}
