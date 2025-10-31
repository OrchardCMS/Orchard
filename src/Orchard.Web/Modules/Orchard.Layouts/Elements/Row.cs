using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Layouts.Elements
{
    public class Row : Container
    {

        public override string Category => "Layout";

        public override LocalizedString DisplayText => T("Row");

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;

        public IEnumerable<Column> Columns => Elements.Cast<Column>();

        public int Size => Columns.Sum(x => x.Size);
    }
}