using System.Collections.Generic;
using Orchard.Rules.Models;

namespace Orchard.Rules.ViewModels {

    public class AddEventViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<EventDescriptor>> Events { get; set; }
    }
}
