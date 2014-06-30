using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class SubscriptionsViewModel {
        public IList<SubscriptionSummaryViewModel> Subscriptions { get; set; }
        public dynamic Pager { get; set; }
    }

    public enum SubscriptionsOrder {
        Description
    }
}
