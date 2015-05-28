using Newtonsoft.Json;

namespace Orchard.DynamicForms.ViewModels
{
    public class ReCaptchaElementResponseModel {
        public bool success { get; set; }

        [JsonProperty("error-codes")]
        public string[] errorMessage { get; set; }
    }
}