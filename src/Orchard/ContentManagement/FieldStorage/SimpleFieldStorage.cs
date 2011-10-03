using System;
using System.Globalization;

namespace Orchard.ContentManagement.FieldStorage {
    public class SimpleFieldStorage : IFieldStorage {
        public SimpleFieldStorage(Func<string, Type, string> getter, Action<string, Type, string> setter) {
            Getter = getter;
            Setter = setter;
        }

        public Func<string, Type, string> Getter { get; set; }
        public Action<string, Type, string> Setter { get; set; }

        public T Get<T>(string name) {
            var value = Getter(name, typeof(T));
            return string.IsNullOrEmpty(value)
                       ? default(T)
                       : (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public void Set<T>(string name, T value) {
            Setter(name, typeof(T), Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }
}