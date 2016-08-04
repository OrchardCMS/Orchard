using Orchard.Core.Common.ViewModels;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorViewModel : Shape {
        public virtual DateTimeEditor Editor { get; set; }
    }
}