namespace Orchard.AntiSpam.ViewModels {
    public class ReCaptchaPartEditViewModel {
        public string PublicKey { get; set; }
    }

    public class ReCaptchaPartSubmitViewModel {
        public string recaptcha_challenge_field { get; set; }
        public string recaptcha_response_field { get; set; }
    }
}