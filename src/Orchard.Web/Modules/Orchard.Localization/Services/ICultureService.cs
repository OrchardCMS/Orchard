using Orchard.Environment.Extensions;

namespace Orchard.Localization.Services {
    public interface ICultureService : IDependency {
        void SetCulture(string culture);
        string GetCulture();
    }

    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CultureService : ICultureService {
        private readonly ICultureStorage _cultureStorage;

        public CultureService(ICultureStorage cultureStorage) {
            _cultureStorage = cultureStorage;
        }
        public void SetCulture(string culture) {
            _cultureStorage.SetCulture(culture);
        }

        public string GetCulture() {
            return _cultureStorage.GetCulture();
        }
    }
}