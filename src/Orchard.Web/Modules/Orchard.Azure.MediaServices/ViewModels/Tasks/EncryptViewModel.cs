namespace Orchard.Azure.MediaServices.ViewModels.Tasks {
    public class EncryptViewModel {
        public bool AdjustSubSamples { get; set; }
        public string ContentKey { get; set; }
        public string CustomAttributes { get; set; }
        public string DataFormats { get; set; }
        public string KeyId { get; set; }
        public string KeySeedValue { get; set; } // This should propably come from site settings.
        public string LicenseAcquisitionUrl { get; set; } // This should propably come from site settings.
        public bool UseSencBox { get; set; }
        public string ServiceId { get; set; }
	}
}