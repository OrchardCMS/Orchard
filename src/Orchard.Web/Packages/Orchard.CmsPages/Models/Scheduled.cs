using System;

namespace Orchard.CmsPages.Models {
    public enum ScheduledAction {
        Undefined,
        Publish,
        Unpublish
    }
    public class Scheduled {
        public virtual int Id { get; set; }
        public virtual Page Page { get; set; }
        public virtual PageRevision PageRevision { get; set; }
        public virtual ScheduledAction Action { get; set; }

        public virtual DateTime? ScheduledDate { get; set; }
    }
}
