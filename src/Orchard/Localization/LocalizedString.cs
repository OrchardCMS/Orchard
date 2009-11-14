namespace Orchard.Localization {
    public class LocalizedString {
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
    }
}
