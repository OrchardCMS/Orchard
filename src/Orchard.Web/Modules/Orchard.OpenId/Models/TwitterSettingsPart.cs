using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class TwitterSettingsPart : ContentPart {

        public string ConsumerKey {
            get { return this.Retrieve(x => x.ConsumerKey, () => Constants.Twitter.DefaultConsumerKey); }
            set { this.Store(x => x.ConsumerKey, value); }
        }

        public string ConsumerSecret {
            get { return this.Retrieve(x => x.ConsumerSecret, () => Constants.Twitter.DefaultConsumerSecret); }
            set { this.Store(x => x.ConsumerSecret, value); }
        }

        public bool IsValid() {
            if (String.IsNullOrWhiteSpace(ConsumerKey) ||
                String.CompareOrdinal(ConsumerKey, Constants.Twitter.DefaultConsumerKey) == 0 ||
                String.IsNullOrWhiteSpace(ConsumerSecret) ||
                String.CompareOrdinal(ConsumerSecret, Constants.Twitter.DefaultConsumerSecret) == 0) {

                return false;
            }

            return true;
        }

        public string VeriSignClass3SecureServerCA_G2
        {
            get { return this.Retrieve(x => x.VeriSignClass3SecureServerCA_G2, () => Constants.Twitter.DefaultVeriSignClass3SecureServerCA_G2); }
            set { this.Store(x => x.VeriSignClass3SecureServerCA_G2, value); }
        }

        public string VeriSignClass3SecureServerCA_G3
        {
            get { return this.Retrieve(x => x.VeriSignClass3SecureServerCA_G3, () => Constants.Twitter.DefaultVeriSignClass3SecureServerCA_G3); }
            set { this.Store(x => x.VeriSignClass3SecureServerCA_G3, value); }
        }

        public string VeriSignClass3PublicPrimaryCA_G5
        {
            get { return this.Retrieve(x => x.VeriSignClass3PublicPrimaryCA_G5, () => Constants.Twitter.DefaultVeriSignClass3PublicPrimaryCA_G5); }
            set { this.Store(x => x.VeriSignClass3PublicPrimaryCA_G5, value); }
        }

        public string SymantecClass3SecureServerCA_G4
        {
            get { return this.Retrieve(x => x.SymantecClass3SecureServerCA_G4, () => Constants.Twitter.DefaultSymantecClass3SecureServerCA_G4); }
            set { this.Store(x => x.SymantecClass3SecureServerCA_G4, value); }
        }

        public string DigiCertSHA2HighAssuranceServerCA
        {
            get { return this.Retrieve(x => x.DigiCertSHA2HighAssuranceServerCA, () => Constants.Twitter.DefaultDigiCertSHA2HighAssuranceServerCA); }
            set { this.Store(x => x.DigiCertSHA2HighAssuranceServerCA, value); }
        }

        public string DigiCertHighAssuranceEVRootCA
        {
            get { return this.Retrieve(x => x.DigiCertHighAssuranceEVRootCA, () => Constants.Twitter.DefaultDigiCertHighAssuranceEVRootCA); }
            set { this.Store(x => x.DigiCertHighAssuranceEVRootCA, value); }
        }
    }
}