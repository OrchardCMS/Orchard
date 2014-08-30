using System.Globalization;

namespace Orchard.Localization.Services {
    public interface ITranslationService : IDependency {
        string Translate(CultureInfo from, CultureInfo to, string value);
    }

    public class DefaultTranslationService : ITranslationService {
        public string Translate(CultureInfo from, CultureInfo to, string value) {
            return value;
        }
    }
}