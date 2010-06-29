using System;
using System.Globalization;

namespace Orchard.ContentManagement.FieldStorage {
    public class SimpleFieldStorage : IFieldStorage {
        public SimpleFieldStorage(Func<string, string> getter, Action<string, string> setter) {
            Getter = getter;
            Setter = setter;
        }

        public Func<string, string> Getter { get; set; }
        public Action<string, string> Setter { get; set; }

        public T Get<T>(string name) {
            var value = Getter(name);
            return string.IsNullOrEmpty(value)
                       ? default(T)
                       : (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public void Set<T>(string name, T value) {
            Setter(name, Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }
}