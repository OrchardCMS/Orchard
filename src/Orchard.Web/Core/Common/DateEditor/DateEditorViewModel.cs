using Orchard.DisplayManagement.Shapes;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorViewModel : Shape {
        public virtual string CreatedDate { get; set; }
        public virtual string CreatedTime { get; set; }
    }
}