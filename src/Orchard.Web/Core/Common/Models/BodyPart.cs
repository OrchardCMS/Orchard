using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class BodyPart : ContentPart<BodyPartRecord> {
        public string Text {
            get { return Get("Text"); }
            set {
                Set("Text", value);
                Record.Text = value;
            }
        }
    }
}
