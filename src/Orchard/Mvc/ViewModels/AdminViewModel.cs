using System.Collections.Generic;
using Orchard.UI.Navigation;

namespace Orchard.Mvc.ViewModels {
    public class AdminViewModel : BaseViewModel {
        public IEnumerable<MenuItem> AdminMenu { get; set; }
    }
}
