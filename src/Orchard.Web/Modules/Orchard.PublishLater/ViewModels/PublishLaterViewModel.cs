using Orchard.ContentManagement;
using Orchard.Core.Common.ViewModels;
using Orchard.PublishLater.Models;

namespace Orchard.PublishLater.ViewModels
{
    public class PublishLaterViewModel
    {
        private readonly PublishLaterPart _publishLaterPart;

        public PublishLaterViewModel(PublishLaterPart publishLaterPart)
        {
            _publishLaterPart = publishLaterPart;
        }

        public ContentItem ContentItem => _publishLaterPart.ContentItem;

        public bool IsPublished => ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published;

        public bool HasDraft
        {
            get
            {
                return (
                    (ContentItem.VersionRecord != null)
                    && ((ContentItem.VersionRecord.Published == false)
                        || (ContentItem.VersionRecord.Published && ContentItem.VersionRecord.Latest == false)));
            }
        }

        public bool HasPublished => IsPublished || ContentItem.ContentManager.Get(ContentItem.Id, VersionOptions.Published) != null;

        public DateTimeEditor Editor { get; set; }
    }
}