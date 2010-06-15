using System.ComponentModel.DataAnnotations;
using Orchard.Core.Common.Fields;

namespace Orchard.Core.Common.ViewModels {
    public class TextContentFieldEditorViewModel {
        public TextContentField TextContentField { get; set; }

        [Required]
        public string Text {
            get { return TextContentField.TextField; }
            set { TextContentField.TextField = value; }
        }
    }
}