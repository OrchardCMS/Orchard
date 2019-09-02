using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class BodyEditorViewModel {
        public BodyPart BodyPart { get; set; }

        public string Text {
            get { return BodyPart.Text; }
            set { BodyPart.Text = value; }
        }

        public string Format {
            get { return BodyPart.Format; }
            set { BodyPart.Format = value; }
        }

        public string EditorFlavor { get; set; }
        public string AddMediaPath { get; set; }
    }
}