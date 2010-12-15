using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPostPart : ContentPart {
        public string Title {
            get { return this.As<RoutePart>().Title; }
            set { this.As<RoutePart>().Title = value; }
        }

        public string Slug {
            get { return this.As<RoutePart>().Slug; }
            set { this.As<RoutePart>().Slug = value; }
        }

        public string Text {
            get { return this.As<BodyPart>().Text; }
            set { this.As<BodyPart>().Text = value; }
        }

        public BlogPart BlogPart {
            get { return this.As<ICommonPart>().Container.As<BlogPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public IUser Creator {
            get { return this.As<ICommonPart>().Owner; }
            set { this.As<ICommonPart>().Owner = value; }
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

        public DateTime? PublishedUtc {
            get { return this.As<ICommonPart>().PublishedUtc; }
        }
    }
}