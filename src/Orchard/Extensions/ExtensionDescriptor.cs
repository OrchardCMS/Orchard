namespace Orchard.Extensions {
    public class ExtensionDescriptor {
        public string Location { get; set; }
        public string Name { get; set; }
        public string ExtensionType { get; set; }
        
        // extension metadata
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string HomePage { get; set; }
    }
}
