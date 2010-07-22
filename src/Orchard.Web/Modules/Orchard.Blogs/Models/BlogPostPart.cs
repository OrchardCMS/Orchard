using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPostPart : ContentPart {
        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return ContentItem.Id; }
        }

        public string Title {
            get { return this.As<IsRoutable>().Title; }
            set { this.As<IsRoutable>().Title = value; }
        }

        public string Slug {
            get { return this.As<IsRoutable>().Slug; }
            set { this.As<IsRoutable>().Slug = value; }
        }

        public string Text {
            get { return this.As<BodyAspect>().Text; }
            set { this.As<BodyAspect>().Text = value; }
        }

        public BlogPart BlogPart {
            get { return this.As<ICommonAspect>().Container.As<BlogPart>(); }
            set { this.As<ICommonAspect>().Container = value; }
        }

        public IUser Creator {
            get { return this.As<ICommonAspect>().Owner; }
            set { this.As<ICommonAspect>().Owner = value; }
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

        public DateTime? CreatedUtc {
            get { return this.As<ICommonAspect>().CreatedUtc; }
        }

        public DateTime? ScheduledPublishUtc { get; set; }

        private string _scheduledPublishUtcDate;

        public string ScheduledPublishUtcDate
        {
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