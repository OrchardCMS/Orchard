namespace Orchard.Extensions {
    public class FeatureDescriptor {
        public string ExtensionName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string[] Dependencies { get; set; }
    }
}
