using Newtonsoft.Json;

namespace Orchard.AntiSpam.ViewModels {
    public class ReCaptchaPartEditViewModel {
        public string PublicKey { get; set; }
    }

    public class ReCaptchaPartResponseModel {
        public bool success { get; set; }

        [JsonProperty("error-codes")]
        public string[] errorMessage { get; set; }
    }
}