namespace Orchard.Azure.MediaServices.ViewModels.Settings {
    public class SettingsViewModel {
        public GeneralSettingsViewModel General { get; set; }
        public EncodingSettingsViewModel EncodingSettings { get; set; }
        public EncryptionSettingsViewModel EncryptionSettings { get; set; }
        public SubtitleLanguagesSettingsViewModel SubtitleLanguages { get; set; }
    }
}