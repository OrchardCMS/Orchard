using System;

namespace Orchard.Localization {
    public class LocalizedString : MarshalByRefObject {
        private readonly string _localized;

        public LocalizedString(string localized) {
            _localized = localized;
        }

        public static implicit operator LocalizedString(string x) {
            return new LocalizedString(x);
        }

        public override string ToString() {
            return _localized;
        }

        public override int GetHashCode() {
            var hashCode = 0;
            if (_localized != null)
                hashCode ^= _localized.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var that = (LocalizedString) obj;
            return string.Equals(_localized, that._localized);
        }
    }
}
