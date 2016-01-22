namespace Orchard.Search.Settings {
    public class ContentPickerSearchFieldSettings {
        public ContentPickerSearchFieldSettings() {
            ShowSearchTab = true;
        }

        public bool ShowSearchTab { get; set; }
        public string SearchIndex { get; set; }
        public string DisplayedContentTypes { get; set; }
    }
}
