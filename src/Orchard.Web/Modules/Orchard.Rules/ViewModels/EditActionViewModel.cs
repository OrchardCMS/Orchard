using Orchard.Rules.Models;

namespace Orchard.Rules.ViewModels {

    public class EditActionViewModel {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public ActionDescriptor Action { get; set; }
        public dynamic Form { get; set; }
    }
}
