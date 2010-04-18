namespace Orchard.Environment.Extensions.Models {
    public class FeatureDescriptor {
        public ExtensionDescriptor Extension { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string[] Dependencies { get; set; }
    }
}