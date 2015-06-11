using System;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Providers.AuditTrail;
using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailEventDescriptorSettingViewModel {
        public AuditTrailEventDescriptor Descriptor { get; set; }
        public AuditTrailEventSettingEventData Setting { get; set; }

        public string EventDisplayName {
            get { return 
                !String.IsNullOrWhiteSpace(Setting.EventDisplayName)
                ? Setting.EventDisplayName
                : Descriptor != null 
                    ? Descriptor.Name.Text
                    : EventNameExtensions.GetShortEventName(Setting.EventName);
            }
        }

        public string EventCategory {
            get {
                return
                    !String.IsNullOrWhiteSpace(Setting.EventCategory)
                    ? Setting.EventCategory 
                    : Descriptor != null
                        ? Descriptor.CategoryDescriptor.Name.Text
                        : Setting.EventCategory;
            }
        }
    }
}