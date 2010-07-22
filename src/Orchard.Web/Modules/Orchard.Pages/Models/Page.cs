using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Security;

namespace Orchard.Pages.Models {
    public class Page : ContentPart {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        public string Title {
            get { return this.As<IsRoutable>().Title; }
        }

        public string Slug {
            get { return this.As<IsRoutable>().Slug; }
            set { this.As<IsRoutable>().Slug = value; }
        }

        public IUser Creator {
            get { return this.As<CommonPart>().Owner; }
            set { this.As<CommonPart>().Owner = value; }
        }

        public bool IsPublished {
            get { return ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published; }
        }

        public bool HasDraft {
            get {
                return (
                           (ContentItem.VersionRecord != null) && (
                               (ContentItem.VersionRecord.Published == false) ||
                               (ContentItem.VersionRecord.Published && ContentItem.VersionRecord.Latest == false)));
            }
        }

        public bool HasPublished {
            get {
                return IsPublished || ContentItem.ContentManager.Get(Id, VersionOptions.Published) != null;
            }
        }
        public string PublishedSlug {
            get {
                if (IsPublished)
                    return Slug;
                Page publishedPage = ContentItem.ContentManager.Get<Page>(Id, VersionOptions.Published);
                if (publishedPage == null) 
                    return String.Empty;
                return publishedPage.Slug;
            }
        }

        public readonly LazyField<DateTime?> _scheduledPublishUtc = new LazyField<DateTime?>();
        public DateTime? ScheduledPublishUtc { get { return _scheduledPublishUtc.Value; } set{ _scheduledPublishUtc.Value = value;} }

        private string _scheduledPublishUtcDate;

        public string ScheduledPublishUtcDate {
            get {
                return !HasPublished && !string.IsNullOrEmpty(_scheduledPublishUtcDate) || !ScheduledPublishUtc.HasValue
                           ? _scheduledPublishUtcDate
                           : ScheduledPublishUtc.Value.ToShortDateString();
            }
            set { _scheduledPublishUtcDate = value; }
        }

        private string _scheduledPublishUtcTime;

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
