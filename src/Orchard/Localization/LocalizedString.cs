using System;
using System.Web;

namespace Orchard.Localization {
    public class LocalizedString : MarshalByRefObject, IHtmlString {
        private readonly string _localized;

        public LocalizedString(string localized) {
            _localized = localized;
        }

        public static LocalizedString TextOrDefault(string text, LocalizedString defaultValue) {
            if (string.IsNullOrEmpty(text))
                return defaultValue;
            else
                return new LocalizedString(text);
        }

        public string Text {
            get { return _localized; }
        }

        public override string ToString() {
            return _localized;
        }

        string IHtmlString.ToHtmlString() {
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

            var that = (LocalizedString)obj;
            return string.Equals(_localized, that._localized);
        }

    }
}
