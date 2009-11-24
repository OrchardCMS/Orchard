using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.UI.Notify;

namespace Orchard.Tags.Services {
    public interface ITagService : IDependency {
        IEnumerable<Tag> GetTags();
        Tag GetTagByName(string tagName);
        void CreateTag(string tagName);
        void TagContentItem(int contentItemId, string tagName);
        void UpdateTagsForContentItem(int contentItemId, IEnumerable<int> tagsForContentItem);
    }

    public class TagService : ITagService {
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<TagsContentItems> _tagsContentItemsRepository;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public TagService(IRepository<Tag> tagRepository, 
                          IRepository<TagsContentItems> tagsContentItemsRepository,
                          IAuthorizer authorizer, INotifier notifier) {
            _tagRepository = tagRepository;
            _tagsContentItemsRepository = tagsContentItemsRepository;
            _authorizer = authorizer;
            _notifier = notifier;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public ISite CurrentSite { get; set; }
        public Localizer T { get; set; }

        #region ITagService Members

        public IEnumerable<Tag> GetTags() {
            return from tags in _tagRepository.Table.ToList() select tags;
        }

        public Tag GetTagByName(string tagName) {
            return _tagRepository.Get(x => x.TagName == tagName);
        }

        public void CreateTag(string tagName) {
            if (_tagRepository.Get(x => x.TagName == tagName) == null) {
                Tag tag = new Tag { TagName = tagName };
                _tagRepository.Create(tag);
            }
            else {
                _notifier.Warning(T("Couldn't create tag: " + tagName + "it already exixts"));
            }
        }

        public void TagContentItem(int contentItemId, string tagName) {
            Tag tag = GetTagByName(tagName);
            TagsContentItems tagsContentItems = new TagsContentItems { ContentItemId = contentItemId, TagId = tag.Id };
            _tagsContentItemsRepository.Create(tagsContentItems);
        }

        public void UpdateTagsForContentItem(int contentItemId, IEnumerable<int> tagsForContentItem) {
            List<int> newTagsForContentItem = new List<int>(tagsForContentItem);
            IEnumerable<TagsContentItems> currentTagsForContentItem = _tagsContentItemsRepository.Fetch(x => x.ContentItemId == contentItemId);
            foreach (var tagContentItem in currentTagsForContentItem) {
                if (!newTagsForContentItem.Contains(tagContentItem.TagId)) {
                    _tagsContentItemsRepository.Delete(tagContentItem);
                }
                else {
                    newTagsForContentItem.Remove(tagContentItem.TagId);
                }
            }
            foreach (var newTagForContentItem in newTagsForContentItem) {
                _tagsContentItemsRepository.Create(new TagsContentItems { ContentItemId = contentItemId, TagId = newTagForContentItem });
            }
        }

        #endregion
    }
}
