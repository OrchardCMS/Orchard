using System;

namespace Orchard.Azure.MediaServices.Models.Assets {
    public class SubtitleAsset : Asset {

        public string Language {
            get { return Storage.Get<string>("Language"); }
            set { Storage.Set("Language", value); }
        }
    }
}