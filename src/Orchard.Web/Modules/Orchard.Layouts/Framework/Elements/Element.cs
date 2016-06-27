using Orchard.Layouts.Elements;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Framework.Elements {
    public abstract class Element : IElement {
        protected Element() {
            T = NullLocalizer.Instance;
            Data = new ElementDataDictionary();
            ExportableData = new ElementDataDictionary();
        }

        public Container Container { get; set; }

        public virtual bool IsSystemElement {
            get { return false; }
        }

        public virtual bool HasEditor {
            get { return true; }
        }

        public virtual string Type {
            get { return GetType().FullName; }
        }

        public virtual LocalizedString DisplayText {
            get { return T(GetType().Name.CamelFriendly()); }
        }

        public virtual LocalizedString Description {
            get { return T("{0} element.", DisplayText); }
        }

        public virtual string ToolboxIcon {
            get { return "\uf1c9"; }
        }

        public abstract string Category { get; }
        public Localizer T { get; set; }
        public string HtmlId { get; set; }
        public string HtmlClass { get; set; }
        public string HtmlStyle { get; set; }
        public string Rule { get; set; }
        public ElementDataDictionary ExportableData { get; set; }
        public ElementDescriptor Descriptor { get; set; }
        public ElementDataDictionary Data { get; set; }
        public bool IsTemplated { get; set; }
        public int Index { get; set; }
    }
}