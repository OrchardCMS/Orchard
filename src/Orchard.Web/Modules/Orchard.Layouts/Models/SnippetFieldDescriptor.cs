using Orchard.Localization;

namespace Orchard.Layouts.Models {
    public class SnippetFieldDescriptor {
        public string Type { get; set; }
        public string Name { get; set; }
        public LocalizedString DisplayName { get; set; }
        public LocalizedString Description { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(Name);
    }
}