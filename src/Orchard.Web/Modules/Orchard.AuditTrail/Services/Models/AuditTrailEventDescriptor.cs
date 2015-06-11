using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.Localization;

namespace Orchard.AuditTrail.Services.Models {
    public class AuditTrailEventDescriptor {
        public AuditTrailCategoryDescriptor CategoryDescriptor { get; set; }
        public string Event { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public bool IsEnabledByDefault { get; set; }
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Returns a basic descriptor based on an event record.
        /// This is useful in cases where event records were previously stored by providers that are no longer enabled.
        /// </summary>
        public static AuditTrailEventDescriptor Basic(AuditTrailEventRecord record) {
            return new AuditTrailEventDescriptor {
                CategoryDescriptor = new AuditTrailCategoryDescriptor {
                    Category = record.Category,
                    Events = Enumerable.Empty<AuditTrailEventDescriptor>(),
                    Name = new LocalizedString(record.Category)
                },
                Event = record.EventName,
                Name = new LocalizedString(record.EventName)
            };
        }
    }
}