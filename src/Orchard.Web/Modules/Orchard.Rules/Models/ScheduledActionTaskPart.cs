using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Rules.Models {
    public class ScheduledActionTaskPart : ContentPart<ScheduledActionTaskRecord> {
        public virtual IList<ScheduledActionRecord> ScheduledActions {
            get { return Record.ScheduledActions; }
        }
    }
}