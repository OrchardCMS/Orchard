using Newtonsoft.Json;

namespace Orchard.DynamicForms.ViewModels {
    public class ReCaptchaElementResponseModel {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}