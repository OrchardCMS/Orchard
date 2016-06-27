using System;
using System.Web;

namespace Orchard.Localization {

    /// <summary>
    /// An HTML-encoded localized string
    /// </summary>
    public class LocalizedString : MarshalByRefObject, IHtmlString {
        private readonly string _localized;
        private readonly string _scope;
        private readonly string _textHint;
        private readonly object[] _args;

        public LocalizedString(string languageNeutral) {
            _localized = languageNeutral;
            _textHint = languageNeutral;
        }

        public LocalizedString(string localized, string scope, string textHint, object[] args) {
            _localized = localized;
            _scope = scope;
            _textHint = textHint;
            _args = args;
        }

        public static LocalizedString TextOrDefault(string text, LocalizedString defaultValue) {
            if (string.IsNullOrEmpty(text))
                return defaultValue;
            return new LocalizedString(text);
        }

        public string Scope {
            get { return _scope; }
        }

        /// <summary>
        /// The HTML-Encoded original text
        /// </summary>
        public string TextHint {
            get { return _textHint; }
        }

        public object[] Args {
            get { return _args; }
        }

        /// <summary>
        /// The HTML-encoded localized text
        /// </summary>
        public string Text {
            get { return _localized; }
        }

        /// <summary>
        /// The HTML-encoded localized text
        /// </summary>
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

        public override object InitializeLifetimeService() {
            // never expire the cross-AppDomain lease on this object
            return null;
        }
    }
}
