using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Framework.Elements {
    public abstract class Element : IElement {
        protected Element() {
            T = NullLocalizer.Instance;
            State = new StateDictionary();
        }

        public IContainer Container { get; set; }

        public virtual bool IsSystemElement {
            get { return false; }
        }

        public virtual bool HasEditor {
            get { return false; }
        }

        public virtual string Type {
            get { return GetType().FullName; }
        }

        public virtual LocalizedString DisplayText {
            get { return T(GetType().Name.CamelFriendly()); }
        }
        public abstract string Category { get; }
        public Localizer T { get; set; }
        public ElementDescriptor Descriptor { get; set; }
        public StateDictionary State { get; set; }
        public bool IsTemplated { get; set; }
        public int Index { get; set; }
    }
}