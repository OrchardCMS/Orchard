using System;
using Orchard.Data.Conventions;

namespace Orchard.AuditTrail.Models {

    /// <summary>
    /// Audit Trail Event Record in the database.
    /// </summary>
    public class AuditTrailEventRecord {
        public virtual int Id { get; set; }

        /// <summary>
        /// The time when the event occurred.
        /// </summary>
        public virtual DateTime CreatedUtc { get; set; }

        /// <summary>
        /// The user name of the user who caused the event to occur.
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public virtual string EventName { get; set; }

        /// <summary>
        /// The full name of the event.
        /// </summary>
        public virtual string FullEventName { get; set; }

        /// <summary>
        /// The category the event belongs to.
        /// </summary>
        public virtual string Category { get; set; }

        /// <summary>
        /// The data of the event.
        /// </summary>
        [StringLengthMax]
        public virtual string EventData { get; set; }

        /// <summary>
        /// The filter key of the event.
        /// </summary>
        public virtual string EventFilterKey { get; set; }

        /// <summary>
        /// The filter data of the event.
        /// </summary>
        public virtual string EventFilterData { get; set; }

        /// <summary>
        /// The comment of the event.
        /// </summary>
        [StringLengthMax]
        public virtual string Comment { get; set; }

        /// <summary>
        /// The IP address of the user who caused the event to occur.
        /// </summary>
        public virtual string ClientIpAddress { get; set; }
    }
}