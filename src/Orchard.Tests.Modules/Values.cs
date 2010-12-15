using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Tests.Modules {
    public static class Values {
        public static IValueProvider From<T>(T obj) {
            if (obj is IDictionary<string,string>) {
                return new DictionaryValueProvider<string>(
                    (IDictionary<string,string>)obj, 
                    CultureInfo.InvariantCulture);
            }
            return new ValueProvider<T>(obj);
        }

        class ValueProvider<T> : IValueProvider {
            private readonly T _obj;

            public ValueProvider(T obj) {
                _obj = obj;
            }

            public bool ContainsPrefix(string prefix) {
                return typeof(T).GetProperties().Any(x => x.Name.StartsWith(prefix));
            }

            public ValueProviderResult GetValue(string key) {
                var property = typeof(T).GetProperty(key);
                if (property == null)
                    return null;
                return new ValueProviderResult(
                    property.GetValue(_obj, null),
                    Convert.ToString(property.GetValue(_obj, null)),
                    null);
            }
        }
    }
}
