using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.Core.Common.Models {
    public class BodyPart : ContentPart<BodyPartRecord> {
        public string Text {
            get { return this.As<InfosetPart>().Get<BodyPart>("Text"); }
            set {
                this.As<InfosetPart>().Set<BodyPart>("Text", value);
                Record.Text = value;
            }
        }
    }
}
