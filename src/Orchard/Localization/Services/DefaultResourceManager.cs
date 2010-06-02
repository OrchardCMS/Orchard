namespace Orchard.Localization.Services {
    public class DefaultResourceManager : IResourceManager {
        // This will use the .po files shortly.
        public string GetLocalizedString(string key, string cultureName) {
            if (cultureName.Equals("en-US")) {
                return key;
            }
            return string.Empty;
        }
    }
}
