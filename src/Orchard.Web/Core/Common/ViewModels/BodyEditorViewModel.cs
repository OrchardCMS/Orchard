using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class BodyEditorViewModel {
        public BodyPart BodyPart { get; set; }

        public string Text {
            get { return BodyPart.Record.Text; }
            set { BodyPart.Record.Text = value; }
        }

        public string Format {
            get { return BodyPart.Record.Format; }
            set { BodyPart.Record.Format = value; }
        }

        public string EditorFlavor { get; set; }
        public string AddMediaPath { get; set; }
    }
}