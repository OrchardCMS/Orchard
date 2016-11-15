using System;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class TwitterSettingsPart : ContentPart {

        public string ConsumerKey {
            get { return this.Retrieve(x => x.ConsumerKey, () => Constants.DefaultTwitterConsumerKey); }
            set { this.Store(x => x.ConsumerKey, value); }
        }

        public string ConsumerSecret {
            get { return this.Retrieve(x => x.ConsumerSecret, () => Constants.DefaultTwitterConsumerSecret); }
            set { this.Store(x => x.ConsumerSecret, value); }
        }

        public bool IsValid {
            get {
                if (String.IsNullOrWhiteSpace(ConsumerKey) ||
                    String.CompareOrdinal(ConsumerKey, Constants.DefaultTwitterConsumerKey) == 0 ||
                    String.IsNullOrWhiteSpace(ConsumerSecret) ||
                    String.CompareOrdinal(ConsumerSecret, Constants.DefaultTwitterConsumerSecret) == 0) {

                    return false;
                }

                return true;
            }
        }
    }
}