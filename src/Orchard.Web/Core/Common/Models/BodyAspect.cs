using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class BodyAspect : ContentPart<BodyRecord> {
        public string Text {
            get { return Record.Text; }
            set { Record.Text = value; }
        }
    }
}
