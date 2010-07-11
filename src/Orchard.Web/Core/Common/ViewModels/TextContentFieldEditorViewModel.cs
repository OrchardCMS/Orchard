using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.Fields;

namespace Orchard.Core.Common.ViewModels {
    public class TextContentFieldEditorViewModel {
        public TextField TextField { get; set; }

        [Required]
        public string Text {
            get { return TextField.Value; }
            set { TextField.Value = value; }
        }
    }
}