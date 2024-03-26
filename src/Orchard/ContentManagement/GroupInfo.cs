using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.ContentManagement {
    public class GroupInfo {
        public GroupInfo(LocalizedString name) {
            Id = name.TextHint.ToSafeName();
            Name = name;
        }

        public string Id { get; set; }
        public LocalizedString Name { get; set; }
        public string Position { get; set; } = "5";
    }
}