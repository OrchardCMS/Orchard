using Newtonsoft.Json;

namespace Orchard.AntiSpam.ViewModels {
    public class ReCaptchaPartEditViewModel {
        public string PublicKey { get; set; }
    }

    public class ReCaptchaPartResponseModel {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}