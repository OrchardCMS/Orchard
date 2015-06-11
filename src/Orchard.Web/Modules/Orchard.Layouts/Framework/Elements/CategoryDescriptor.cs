using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Layouts.Framework.Elements {
    public class CategoryDescriptor {
        public CategoryDescriptor() {
            Elements = new List<ElementDescriptor>();
        }

        public CategoryDescriptor(string name, LocalizedString displayName, LocalizedString description, int position) : this() {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Position = position;
        }

        public string Name { get; set; }
        public LocalizedString DisplayName { get; set; }
        public LocalizedString Description { get; set; }
        public int Position { get; set; }
        public IList<ElementDescriptor> Elements { get; set; }
    }
}