using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.PublishLater.Models;

namespace Orchard.PublishLater.ViewModels {
    public class PublishLaterViewModel {
        private readonly PublishLaterPart _publishLaterPart;

        public PublishLaterViewModel(PublishLaterPart publishLaterPart) {
            _publishLaterPart = publishLaterPart;
        }

        public ContentItem ContentItem { get { return _publishLaterPart.ContentItem; } }

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

        public DateTime? VersionPublishedUtc { get { return ContentItem.As<CommonPart>() == null ? null : ContentItem.As<CommonPart>().VersionPublishedUtc; } }

        public DateTime? ScheduledPublishUtc { get; set; }

        public string ScheduledPublishDate { get; set; }

        public string ScheduledPublishTime { get; set; }
    }
}