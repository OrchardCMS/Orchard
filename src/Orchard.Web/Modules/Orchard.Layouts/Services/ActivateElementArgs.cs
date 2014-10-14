using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public class ActivateElementArgs {
        public static readonly ActivateElementArgs Empty = new ActivateElementArgs();
        public IContainer Container { get; set; }
        public int Index { get; set; }
        public StateDictionary ElementState { get; set; }
    }
}