using Orchard.Environment.Extensions;

namespace Orchard.Localization.Services {
    public interface ICultureService : IDependency {
        void SetCulture(string culture);
        string GetCulture();
    }

    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CultureService : ICultureService {
        private readonly ICultureStorageProvider _cultureStorageProvider;

        public CultureService(ICultureStorageProvider cultureStorageProvider) {
            _cultureStorageProvider = cultureStorageProvider;
        }
        public void SetCulture(string culture) {
            _cultureStorageProvider.SetCulture(culture);
        }

        public string GetCulture() {
            return _cultureStorageProvider.GetCulture();
        }
    }
}