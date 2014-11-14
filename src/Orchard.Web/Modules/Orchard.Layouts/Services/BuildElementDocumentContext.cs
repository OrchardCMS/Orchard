using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Services {
    public class BuildElementDocumentContext {
        public IElement Element { get; set; }
        public string HtmlContent { get; set; }
        public ILayoutAspect Layout { get; set; }
    }
}