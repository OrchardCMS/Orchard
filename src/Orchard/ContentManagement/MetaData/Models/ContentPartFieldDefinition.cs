namespace Orchard.ContentManagement.MetaData.Models {
    public class ContentPartFieldDefinition {
        public ContentPartFieldDefinition(string name) {
            Name = name;
            FieldDefinition = new ContentFieldDefinition(null);
            Settings = new SettingsDictionary();
        }
        public ContentPartFieldDefinition( ContentFieldDefinition contentFieldDefinition, string name, SettingsDictionary settings) {
            Name = name;
            FieldDefinition = contentFieldDefinition;
            Settings = settings;
        }

        public string Name { get; private set; }
        public ContentFieldDefinition FieldDefinition { get; private set; }
        public SettingsDictionary Settings { get; private set; }
    }
}