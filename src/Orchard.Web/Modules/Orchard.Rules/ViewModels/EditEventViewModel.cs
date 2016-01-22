using Orchard.Rules.Models;

namespace Orchard.Rules.ViewModels {

    public class EditEventViewModel {
        public int Id { get; set; }
        public int EventId { get; set; }
        public EventDescriptor Event { get; set; }
        public dynamic Form { get; set; }
    }
}
