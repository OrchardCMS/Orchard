using Orchard.Localization;

namespace Orchard.Layouts.Framework.Elements {
    public interface IElement {
        IContainer Container { get; set; }
        string Type { get; }
        LocalizedString DisplayText { get; }
        string Category { get; }
        bool IsSystemElement { get; }
        bool HasEditor { get; }
        bool IsTemplated { get; set; }
        StateDictionary State { get; set; }
        ElementDescriptor Descriptor { get; set; }
        int Index { get; set; }
        Localizer T { get; set; }
    }
}