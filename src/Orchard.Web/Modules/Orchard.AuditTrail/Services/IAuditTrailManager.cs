using System;
using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailManager : IDependency {
        IPageOfItems<AuditTrailEventRecord> GetPage(int page, int pageSize);
        AuditTrailEventRecord GetRecord(int id);
        AuditTrailEventRecord Record(string eventName, IUser user, IContent content = null, IDictionary<string, object> eventData = null);
        IEnumerable<AuditTrailCategoryDescriptor> Describe();
        dynamic BuildDisplay(AuditTrailEventRecord record, string displayType);
    }
}