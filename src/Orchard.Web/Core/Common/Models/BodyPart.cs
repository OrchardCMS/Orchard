using Orchard.Models;

namespace Orchard.Core.Common.Models {
    public class BodyPart : Orchard.Models.ContentPart {
        public string Body { get; set; }
        public string Format { get; set; }
    }
}
