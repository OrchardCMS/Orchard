using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements
{
    public class Column : Container
    {

        public override string Category => "Layout";

        public override LocalizedString DisplayText => T("Column");

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;

        public int? Width
        {
            get { return this.Retrieve<int?>("Width") ?? this.Retrieve<int?>("ColumnSpan") ?? 12; } // Falling back on "ColumnSpan" for backward compatibility.
            set { this.Store(x => x.Width, value); }
        }

        public int? Offset
        {
            get { return this.Retrieve<int?>("Offset") ?? this.Retrieve<int?>("ColumnOffset") ?? 0; } // Falling back on "ColumnOffset" for backward compatibility.
            set { this.Store(x => x.Offset, value); }
        }

        public int Size => Width.GetValueOrDefault() + Offset.GetValueOrDefault();

        public bool? Collapsible
        {
            get { return this.Retrieve(x => x.Collapsible); }
            set { this.Store(x => x.Collapsible, value); }
        }
    }
}