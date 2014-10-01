using System;
using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services.Models;
using Orchard.Collections;
using Orchard.Security;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailManager : IDependency {
        /// <summary>
        /// Gets a page of event records from the audit trail.
        /// </summary>
        /// <param name="page">The page number to get records from.</param>
        /// <param name="pageSize">The number of records to get.</param>
        /// <param name="orderBy">The value to order by.</param>
        /// <param name="filters">Optional. An object to filter the records on.</param>
        /// <returns>A page of event records.</returns>
        IPageOfItems<AuditTrailEventRecord> GetRecords(int page, int pageSize, Filters filters = null, AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending);

        /// <summary>
        /// Gets a single event record from the audit trail by ID.
        /// </summary>
        /// <param name="id">The event record ID.</param>
        /// <returns>A single event record.</returns>
        AuditTrailEventRecord GetRecord(int id);

        /// <summary>
        /// Builds a shape tree of filter displays.
        /// </summary>
        /// <param name="filters">Input for each filter builder.</param>
        /// <returns>A tree of shapes.</returns>
        dynamic BuildFilterDisplay(Filters filters);
        
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
        /// <returns>The created audit trail event record if the specified event was not disabled.</returns>
        AuditTrailEventRecordResult CreateRecord<T>(string eventName, IUser user, IDictionary<string, object> properties = null, IDictionary<string, object> eventData = null, string eventFilterKey = null, string eventFilterData = null) where T : IAuditTrailEventProvider;

        /// <summary>
        /// Describes all audit trail events provided by the system.
        /// </summary>
        /// <returns>A list of audit trail category descriptors.</returns>
        IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories();

        /// <summary>
        /// Describes all audit trail event providers.
        /// </summary>
        DescribeContext DescribeProviders();

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <param name="record">The audit trail event record for which to find its descriptor.</param>
        /// <returns>A single audit trail event descriptor.</returns>
        AuditTrailEventDescriptor DescribeEvent(AuditTrailEventRecord record);

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <typeparam name="T">The scope of the specified event name.</typeparam>
        /// <param name="eventName">The shorthand name of the event.</param>
        /// <returns>A single audit trail event descriptor.</returns>
        AuditTrailEventDescriptor DescribeEvent<T>(string eventName) where T : IAuditTrailEventProvider;

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <param name="fullyQualifiedEventName">The fully qualified event name to describe.</param>
        /// <returns>A single audit trail event descriptor.</returns>
        AuditTrailEventDescriptor DescribeEvent(string fullyQualifiedEventName);

        /// <summary>
        /// Trims the audit trail by deleting all records older than the specified retention period.
        /// </summary>
        /// <returns>A list of deleted records.</returns>
        IEnumerable<AuditTrailEventRecord> Trim(TimeSpan retentionPeriod);

        /// <summary>
        /// Serializes the specified list of settings into a string.
        /// </summary>
        string SerializeProviderConfiguration(IEnumerable<AuditTrailEventSetting> settings);

        /// <summary>
        /// Deserializes the specified string into a list of settings.
        /// </summary>
        IEnumerable<AuditTrailEventSetting> DeserializeProviderConfiguration(string data);
    }
}