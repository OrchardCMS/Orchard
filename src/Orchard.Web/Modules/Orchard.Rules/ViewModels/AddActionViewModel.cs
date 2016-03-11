using System.Collections.Generic;
using Orchard.Rules.Models;

namespace Orchard.Rules.ViewModels {

    public class AddActionViewModel {
        public int Id { get; set; }
        public IEnumerable<TypeDescriptor<ActionDescriptor>> Actions { get; set; }
    }
}
