using System;
using System.Collections.Generic;

namespace Orchard.Azure.MediaServices.Models.Assets {
    public class DynamicVideoAsset : VideoAsset {

        public string WamsPrivateOnDemandLocatorId {
            get { return Storage.Get<string>("WamsPrivateOnDemandLocatorId"); }
            set { Storage.Set("WamsPrivateOnDemandLocatorId", value); }
        }

        public string WamsPrivateOnDemandLocatorUrl {
            get { return Storage.Get<string>("WamsPrivateOnDemandLocatorUrl"); }
            set { Storage.Set("WamsPrivateOnDemandLocatorUrl", value); }
        }

        public string WamsPublicOnDemandLocatorId {
            get { return Storage.Get<string>("WamsPublicOnDemandLocatorId"); }
            set { Storage.Set("WamsPublicOnDemandLocatorId", value); }
        }

        public string WamsPublicOnDemandLocatorUrl {
            get { return Storage.Get<string>("WamsPublicOnDemandLocatorUrl"); }
            set { Storage.Set("WamsPublicOnDemandLocatorUrl", value); }
        }

        public string WamsManifestFilename {
            get { return Storage.Get<string>("WamsManifestFilename"); }
            set { Storage.Set("WamsManifestFilename", value); }
        }

        public string PrivateManifestUrl {
            get { return !String.IsNullOrEmpty(WamsPrivateOnDemandLocatorUrl) && !String.IsNullOrEmpty(WamsManifestFilename) ? String.Format("{0}{1}", WamsPrivateOnDemandLocatorUrl, WamsManifestFilename) : null; }
        }

        public string PrivateSmoothStreamingUrl {
            get { return !String.IsNullOrEmpty(PrivateManifestUrl) ? String.Format("{0}/manifest", PrivateManifestUrl) : null; }
        }

        public string PrivateHlsUrl {
            get { return !String.IsNullOrEmpty(PrivateManifestUrl) ? String.Format("{0}/manifest(format=m3u8-aapl)", PrivateManifestUrl) : null; }
        }

        public string PrivateMpegDashUrl {
            get { return !String.IsNullOrEmpty(PrivateManifestUrl) ? String.Format("{0}/manifest(format=mpd-time-csf)", PrivateManifestUrl) : null; }
        }

        public string PublicManifestUrl {
            get { return !String.IsNullOrEmpty(WamsPublicOnDemandLocatorUrl) && !String.IsNullOrEmpty(WamsManifestFilename) ? String.Format("{0}{1}", WamsPublicOnDemandLocatorUrl, WamsManifestFilename) : null; }
        }

        public string PublicSmoothStreamingUrl {
            get { return !String.IsNullOrEmpty(PublicManifestUrl) ? String.Format("{0}/manifest", PublicManifestUrl) : null; }
        }

        public string PublicHlsUrl {
            get { return !String.IsNullOrEmpty(PublicManifestUrl) ? String.Format("{0}/manifest(format=m3u8-aapl)", PublicManifestUrl) : null; }
        }

        public string PublicMpegDashUrl {
            get { return !String.IsNullOrEmpty(PublicManifestUrl) ? String.Format("{0}/manifest(format=mpd-time-csf)", PublicManifestUrl) : null; }
        }

        public override IEnumerable<DisplayLocator> GetDisplayLocators() {
            foreach (var locator in base.GetDisplayLocators())
                yield return locator;

            yield return new DisplayLocator("Private (on-demand)", WamsPrivateOnDemandLocatorId, WamsPrivateOnDemandLocatorUrl);
            yield return new DisplayLocator("Public (on-demand)", WamsPublicOnDemandLocatorId, WamsPublicOnDemandLocatorUrl);
        }
    }
}