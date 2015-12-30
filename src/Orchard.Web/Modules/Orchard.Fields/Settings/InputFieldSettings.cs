namespace Orchard.Fields.Settings {

    public enum InputType {
        Text,
        Url,
        Tel,
        Email
    }

    public class InputFieldSettings {
        public InputType Type { get; set; }
        public string Title { get; set; }
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool AutoFocus { get; set; }
        public bool AutoComplete { get; set; }
        public string Placeholder { get; set; }
        public string Pattern { get; set; }
        public string EditorCssClass { get; set; }
        public int MaxLength { get; set; }
        public string DefaultValue { get; set; }

        public InputFieldSettings() {
            Type = InputType.Text;
        }
    }
}
