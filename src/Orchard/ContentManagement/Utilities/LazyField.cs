using System;

namespace Orchard.ContentManagement.Utilities {
    public class LazyField<T> {
        private T _value;
        private Func<T, T> _loader;
        private Func<T, T> _setter;

        public T Value {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        public void Loader(Func<T, T> loader) {
            _loader = loader;
        }

        public void Setter(Func<T, T> setter) {
            _setter = setter;
        }

        private T GetValue() {
            if (_loader != null) {
                _value = _loader(_value);
                _loader = null;
            }
            return _value;
        }

        private void SetValue(T value) {
            _loader = null;
            if (_setter != null) {
                _value = _setter(value);
            }
            else {
                _value = value;
            }
        }
    }
}
