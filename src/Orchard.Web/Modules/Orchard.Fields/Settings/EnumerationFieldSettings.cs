namespace Orchard.Fields.Settings {

    public enum ListMode {
        Dropdown,
        Radiobutton,
        Listbox,
        Checkbox
    }

    public class EnumerationFieldSettings {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string Options { get; set; }
        public ListMode ListMode { get; set; }
        public string DefaultValue { get; set; }

        public EnumerationFieldSettings() {
            ListMode = ListMode.Dropdown;
        }
    }
}
