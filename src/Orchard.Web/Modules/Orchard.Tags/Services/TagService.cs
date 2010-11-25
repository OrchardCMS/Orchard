using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tags.Models;
using Orchard.UI.Notify;

namespace Orchard.Tags.Services {
    [UsedImplicitly]
    public class TagService : ITagService {
        private readonly IRepository<TagRecord> _tagRepository;
        private readonly IRepository<TagsContentItems> _tagsContentItemsRepository;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrchardServices _orchardServices;

        public TagService(IRepository<TagRecord> tagRepository,
                          IRepository<TagsContentItems> tagsContentItemsRepository,
                          INotifier notifier,
                          IAuthorizationService authorizationService,
                          IOrchardServices orchardServices) {
            _tagRepository = tagRepository;
            _tagsContentItemsRepository = tagsContentItemsRepository;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<TagRecord> GetTags() {
            return _tagRepository.Table.ToList();
        }

        public TagRecord GetTag(int tagId) {
            return _tagRepository.Get(x => x.Id == tagId);
        }

        public TagRecord GetTagByName(string tagName) {
            return _tagRepository.Get(x => x.TagName == tagName);
        }

        public void CreateTag(string tagName) {
            if (_tagRepository.Get(x => x.TagName == tagName) == null) {
                _authorizationService.CheckAccess(Permissions.CreateTag, _orchardServices.WorkContext.CurrentUser, null);

                TagRecord tagRecord = new TagRecord { TagName = tagName };
                _tagRepository.Create(tagRecord);
            }
            else {
                _notifier.Warning(T("The tag {0} already exists", tagName));
            }
        }

        public void DeleteTag(int tagId) {
            _tagRepository.Delete(GetTag(tagId));
            IEnumerable<TagsContentItems> tagsContentItems = _tagsContentItemsRepository.Fetch(x => x.Tag.Id == tagId);
            foreach (var tagContentItem in tagsContentItems) {
                _tagsContentItemsRepository.Delete(tagContentItem);
            }
        }

        public void UpdateTag(int tagId, string tagName) {
            if (String.IsNullOrEmpty(tagName)) {
                _notifier.Warning(T("Couldn't rename tag: name was empty"));
                return;
            }

            var tagRecord = GetTagByName(tagName);

            // new tag name already existing => merge
            if (tagRecord != null) {
                var tagsContentItems = _tagsContentItemsRepository.Fetch(x => x.Tag.Id == tagId);

                // get contentItems already tagged with the existing one
                var taggedContentItems = GetTaggedContentItems(tagRecord.Id);

                foreach (var tagContentItem in tagsContentItems) {
                    if (!taggedContentItems.Any(c => c.ContentItem.Record == tagContentItem.ContentItem)) {
                        TagContentItem(tagContentItem.ContentItem, tagName);
                    }
                    _tagsContentItemsRepository.Delete(tagContentItem);
                }

                _tagRepository.Delete(GetTag(tagId));
                return;
            }

            // Create new tag
            tagRecord = _tagRepository.Get(tagId);
            tagRecord.TagName = tagName;
        }

        public IEnumerable<IContent> GetTaggedContentItems(int tagId) {
            return _tagsContentItemsRepository
                .Fetch(x => x.Tag.Id == tagId)
                .Select(t => _orchardServices.ContentManager.Get(t.ContentItem.Id))
                .Where(c => c != null);
        }

        private void TagContentItem(ContentItemRecord contentItem, string tagName) {
            var tagRecord = GetTagByName(tagName);
            var tagsContentItems = new TagsContentItems { ContentItem = contentItem, Tag = tagRecord };
            _tagsContentItemsRepository.Create(tagsContentItems);
        }

        public void UpdateTagsForContentItem(ContentItem contentItem, IEnumerable<string> tagNamesForContentItem) {
            var tags = new List<TagRecord>();
            foreach (var tagName in tagNamesForContentItem) {
                TagRecord tagRecord = GetTagByName(tagName);
                if (tagRecord == null) {
                    CreateTag(tagName);
                    tagRecord = GetTagByName(tagName);
                }
                tags.Add(tagRecord);
            }
            ModifyTagsForContentItem(contentItem, tags);
        }

        private void ModifyTagsForContentItem(ContentItem contentItem, IEnumerable<TagRecord> tagsForContentItem) {
            var newTagsForContentItem = new List<TagRecord>(tagsForContentItem);
            var currentTagsForContentItem = _tagsContentItemsRepository.Fetch(x => x.ContentItem == contentItem.Record);

            foreach (var tagContentItem in currentTagsForContentItem) {
                if (!newTagsForContentItem.Contains(tagContentItem.Tag)) {
                    _authorizationService.CheckAccess(Permissions.ApplyTag, _orchardServices.WorkContext.CurrentUser, null);

                    _tagsContentItemsRepository.Delete(tagContentItem);
                }
                else {
                    newTagsForContentItem.Remove(tagContentItem.Tag);
                }
            }

            foreach (var newTagForContentItem in newTagsForContentItem) {
                _authorizationService.CheckAccess(Permissions.ApplyTag, _orchardServices.WorkContext.CurrentUser, null);

                _tagsContentItemsRepository.Create(new TagsContentItems { ContentItem = contentItem.Record, Tag = newTagForContentItem });
            }
        }
    }
}
