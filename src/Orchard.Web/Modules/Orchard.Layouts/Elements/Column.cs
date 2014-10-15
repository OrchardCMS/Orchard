using System.Globalization;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Column : Container, IColumn {
        
        public override string Category {
            get { return "Layout"; }
        }

        public override LocalizedString DisplayText {
            get { return T("Column"); }
        }

        public override bool IsSystemElement {
            get { return true; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public int? ColumnSpan {
            get { return State.Get("ColumnSpan").ToInt32() ?? 0; }
            set { State["ColumnSpan"] = value != null ? value.Value.ToString(CultureInfo.InvariantCulture) : null; }
        }

        public int? ColumnOffset {
            get { return State.Get("ColumnOffset").ToInt32() ?? 0; }
            set { State["ColumnOffset"] = value != null ? value.Value.ToString(CultureInfo.InvariantCulture) : null; }
        }

        public int CurrentSpanSize {
            get { return ColumnSpan.GetValueOrDefault() + ColumnOffset.GetValueOrDefault(); }
        }
    }
}