using System.Collections.Generic;
using Orchard.Notify;

namespace Orchard.Mvc.ViewModels {
    public class BaseViewModel {
        public BaseViewModel() {
            Messages = new List<NotifyEntry>();
        }

        public IList<NotifyEntry> Messages { get; set; }
    }
}
