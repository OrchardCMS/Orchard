using System;
using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.Collections;
using Orchard.Security;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailManager : IDependency {
        /// <summary>
        /// Gets a page of event records.
        /// </summary>
        /// <param name="page">The page number to get records from.</param>
        /// <param name="pageSize">The number of records to get.</param>
        /// <param name="orderBy">The value to order by.</param>
        /// <param name="filter">Optional. An object to filter the records on.</param>
        /// <returns>Returns a page of event records.</returns>
        IPageOfItems<AuditTrailEventRecord> GetRecords(int page, int pageSize, AuditTrailFilterParameters filter = null, AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending);

        /// <summary>
        /// Returns a single event record by ID.
        /// </summary>
        /// <param name="id">The event record ID.</param>
        /// <returns>Returns a single event record by ID.</returns>
        AuditTrailEventRecord GetRecord(int id);
        
        /// <summary>
        /// Records an audit trail event.
        /// </summary>
        /// <typeparam name="T">The audit trail event provider type to determine the scope of the event name.</typeparam>
        /// <param name="eventName">The shorthand name of the event</param>
        /// <param name="user">The user to associate with the event. This is typically the currently loggedin user.</param>
        /// <param name="properties">A property bag of custom event data that could be useful for <see cref="IAuditTrailEventHandler"/> implementations. These values aren't stored. Use the eventData parameter to persist additional data with the event.</param>
        /// <param name="eventData">A property bag of custom event data that will be stored with the event record.</param>
        /// <param name="eventFilterKey">The name of a custom key to use when filtering events.</param>
        /// <param name="eventFilterData">The value of a custom filter key to filter on.</param>
        /// <returns>Returns the created audit trail event record if the specified event was not disabled.</returns>
        AuditTrailEventRecordResult Record<T>(string eventName, IUser user, IDictionary<string, object> properties = null, IDictionary<string, object> eventData = null, string eventFilterKey = null, string eventFilterData = null) where T : IAuditTrailEventProvider;

        /// <summary>
        /// Describes all audit trail events provided by the system.
        /// </summary>
        /// <returns>Returns a list of audit trail category descriptors.</returns>
        IEnumerable<AuditTrailCategoryDescriptor> Describe();

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <typeparam name="T">The scope of the specified event name.</typeparam>
        /// <param name="eventName">The shorthand name of the event.</param>
        /// <returns>Returns a single audit trail event descriptor.</returns>
        AuditTrailEventDescriptor Describe<T>(string eventName) where T : IAuditTrailEventProvider;

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <param name="fullyQualifiedEventName">The fully qualified event name to describe.</param>
        /// <returns>Returns a single audit trail event descriptor.</returns>
        AuditTrailEventDescriptor Describe(string fullyQualifiedEventName);
    }
}