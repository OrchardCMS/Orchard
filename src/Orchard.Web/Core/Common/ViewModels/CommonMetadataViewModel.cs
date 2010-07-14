using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Security;

namespace Orchard.Core.Common.ViewModels {
    public class CommonMetadataViewModel {
        private readonly CommonAspect _commonAspect;

        public CommonMetadataViewModel(CommonAspect commonAspect) {
            _commonAspect = commonAspect;
        }

        public IUser Creator { get { return _commonAspect.Owner; } }
        public ContentItem ContentItem { get { return _commonAspect.ContentItem; } }

        public DateTime? CreatedUtc { get { return _commonAspect.CreatedUtc; } }
        public DateTime? PublishedUtc { get { return _commonAspect.PublishedUtc; } }
        public DateTime? ModifiedUtc { get { return _commonAspect.ModifiedUtc; } }

        public DateTime? VersionCreatedUtc { get { return _commonAspect.VersionCreatedUtc; } }
        public DateTime? VersionPublishedUtc { get { return _commonAspect.VersionPublishedUtc; } }
        public DateTime? VersionModifiedUtc { get { return _commonAspect.VersionModifiedUtc; } }

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
    }
}