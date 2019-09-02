namespace Orchard.ContentPicker.Settings {
    public class ContentPickerFieldSettings {
        public ContentPickerFieldSettings() {
            ShowContentTab = true;
        }

        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }

        public bool ShowContentTab { get; set; }
        public string DisplayedContentTypes { get; set; }
    }
}
