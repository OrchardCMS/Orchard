using System;

namespace Orchard.Tokens {
    public class TokenDescriptor<T> : TokenDescriptor {
        public object Value(T context) {
            return ValueGetter(context);
        }

        public TokenDescriptor<T> WithValue(Func<T, object> valueGetter) {
            return WithValue(null, valueGetter);
        }

        public TokenDescriptor<T> WithValue(string valueType, Func<T, object> valueGetter) {
            ValueType = ValueType;
            ValueGetter = o => valueGetter((T)o);
            return this;
        }

        public new TokenDescriptor<T> WithDescription(object description) {
            Description = description;
            return this;
        }
    }

    public class TokenDescriptor {
        protected Func<object, object> ValueGetter { get; set; }
        public string Name { get; set; }
        public object Description { get; set; }
        public string Type { get; set; }
        public string ValueType { get; set; }

        public object Value(object context) {
            return ValueGetter(context);
        }

        public TokenDescriptor WithValue(Func<object, object> valueGetter) {
            return WithValue(null, valueGetter);
        }

        public TokenDescriptor WithValue(string valueType, Func<object, object> valueGetter) {
            ValueType = valueType;
            ValueGetter = valueGetter;
            return this;
        }

        public TokenDescriptor WithDescription(object description) {
            Description = description;
            return this;
        }

    }

}
