using System;
using System.Globalization;

namespace Orchard.Utility {
    /// <summary>
    /// Compute an (almost) unique hash value from various sources.
    /// This allows computing hash keys that are easily storable
    /// and comparable from heterogenous components.
    /// </summary>
    public class Hash {
        private long _hash;

        public string Value { get { return _hash.ToString("x", CultureInfo.InvariantCulture); } }

        public void AddString(string value) {
            if (string.IsNullOrEmpty(value))
                return;
            _hash += value.GetHashCode();
        }

        public void AddTypeReference(Type type) {
            AddString(type.AssemblyQualifiedName);
            AddString(type.FullName);
        }

        public void AddDateTime(DateTime dateTime) {
            _hash += dateTime.ToUniversalTime().ToBinary();
        }
    }
}
