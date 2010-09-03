using System.Web;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class BodyDisplayViewModel {
        public BodyPart BodyPart { get; set; }
        public IHtmlString Html { get; set; }
    }
}