using System.Collections.Generic;
using Orchard.Notify;
using Orchard.Security;

namespace Orchard.Mvc.ViewModels {
    public class BaseViewModel {
        public BaseViewModel() {
            Messages = new List<NotifyEntry>();
        }

        public IList<NotifyEntry> Messages { get; set; }
        public IUser CurrentUser { get; set; }
    }
}
