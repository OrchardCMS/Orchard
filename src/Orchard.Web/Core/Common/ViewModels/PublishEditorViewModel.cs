using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class PublishEditorViewModel {
        private readonly CommonAspect _commonAspect;
        private string _scheduledPublishUtcTime;
        private string _scheduledPublishUtcDate;

        public PublishEditorViewModel(CommonAspect commonAspect) {
            _commonAspect = commonAspect;
        }

        public string Command { get; set; }
        public ContentItem ContentItem { get { return _commonAspect.ContentItem; } }

        public bool IsPublished {
            get { return ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published; }
        }

        public bool HasDraft {
            get {
                return (
                    (ContentItem.VersionRecord != null)
                    && ((ContentItem.VersionRecord.Published == false)
                        || (ContentItem.VersionRecord.Published && ContentItem.VersionRecord.Latest == false)));
            }
        }

        public bool HasPublished {
            get { return IsPublished || ContentItem.ContentManager.Get(ContentItem.Id, VersionOptions.Published) != null; }
        }

        public DateTime? ScheduledPublishUtc { get; set; }

        public string ScheduledPublishUtcDate {
            get {
                return !HasPublished && !string.IsNullOrEmpty(_scheduledPublishUtcDate) || !ScheduledPublishUtc.HasValue
                           ? _scheduledPublishUtcDate
                           : ScheduledPublishUtc.Value.ToShortDateString();
            }
            set { _scheduledPublishUtcDate = value; }
        }


        public string ScheduledPublishUtcTime {
            get {
                return !HasPublished && !string.IsNullOrEmpty(_scheduledPublishUtcTime) || !ScheduledPublishUtc.HasValue
                           ? _scheduledPublishUtcTime
                           : ScheduledPublishUtc.Value.ToShortTimeString();
            }
            set { _scheduledPublishUtcTime = value; }
        }
    }
}