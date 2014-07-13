namespace Orchard.Azure.MediaServices.Services.Wams {
    public class WamsLocators {
        public WamsLocators(WamsLocatorInfo sasLocator, WamsLocatorInfo onDemandLocator, string onDemandManifestFilename) {
            SasLocator = sasLocator;
            OnDemandLocator = onDemandLocator;
            OnDemandManifestFilename = onDemandManifestFilename;
        }

        public WamsLocatorInfo SasLocator { get; private set; }
        public WamsLocatorInfo OnDemandLocator { get; private set; }
        public string OnDemandManifestFilename { get; private set; }
    }
}