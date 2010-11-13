using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<TagsContentItems> _tagsContentItemsRepository;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrchardServices _orchardServices;

        public TagService(IRepository<Tag> tagRepository,
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

        public IEnumerable<Tag> GetTags() {
            return _tagRepository.Table.ToList();
        }

        public Tag GetTag(int id) {
            return _tagRepository.Get(x => x.Id == id);
        }

        public Tag GetTagByName(string tagName) {
            return _tagRepository.Get(x => x.TagName == tagName);
        }

        public void CreateTag(string tagName) {
            if (_tagRepository.Get(x => x.TagName == tagName) == null) {
                _authorizationService.CheckAccess(Permissions.CreateTag, _orchardServices.WorkContext.CurrentUser, null);

                Tag tag = new Tag { TagName = tagName };
                _tagRepository.Create(tag);
            }
            else {
                _notifier.Warning(T("The tag {0} already exists", tagName));
            }
        }

        public void DeleteTag(int id) {
            _tagRepository.Delete(GetTag(id));
            IEnumerable<TagsContentItems> tagsContentItems = _tagsContentItemsRepository.Fetch(x => x.TagId == id);
            foreach (var tagContentItem in tagsContentItems) {
                _tagsContentItemsRepository.Delete(tagContentItem);
            }
        }

        public void UpdateTag(int id, string tagName) {
            if ( String.IsNullOrEmpty(tagName) ) {
                _notifier.Warning(T("Couldn't rename tag: name was empty"));
                return;
            }

            Tag tag = GetTagByName(tagName);
            if(tag != null) {
                // new tag name already existing => merge
                IEnumerable<TagsContentItems> tagsContentItems = _tagsContentItemsRepository.Fetch(x => x.TagId == id);
                
                // get contentItems already tagged with the existing one
                var taggedContentItems = GetTaggedContentItems(tag.Id);

                foreach ( var tagContentItem in tagsContentItems ) {
                    var tagContentItemId = tagContentItem.ContentItemId;
                    if ( !taggedContentItems.Any(c => c.ContentItem.Id == tagContentItemId) ) {
                        TagContentItem(tagContentItem.ContentItemId, tagName);
                    }
                    _tagsContentItemsRepository.Delete(tagContentItem);
                }

                _tagRepository.Delete(GetTag(id));
            }
            else {
                tag = _tagRepository.Get(id);
                tag.TagName = tagName;
            }
        }

        public IEnumerable<IContent> GetTaggedContentItems(int id) {
            return _tagsContentItemsRepository
                .Fetch(x => x.TagId == id)
                .Select(t =>_orchardServices.ContentManager.Get(t.ContentItemId))
                .Where(c => c!= null);
        }

        public void TagContentItem(int contentItemId, string tagName) {
            Tag tag = GetTagByName(tagName);
            TagsContentItems tagsContentItems = new TagsContentItems { ContentItemId = contentItemId, TagId = tag.Id };
            _tagsContentItemsRepository.Create(tagsContentItems);
        }

        public void UpdateTagsForContentItem(int contentItemId, IEnumerable<string> tagNamesForContentItem) {
            List<int> tags = new List<int>();
            foreach (var tagName in tagNamesForContentItem) {
                Tag tag = GetTagByName(tagName);
                if (tag == null) {
                    CreateTag(tagName);
                    tag = GetTagByName(tagName);
                }
                tags.Add(tag.Id);
            }
            ModifyTagsForContentItem(contentItemId, tags);
        }

        private void ModifyTagsForContentItem(int contentItemId, IEnumerable<int> tagsForContentItem) {
            List<int> newTagsForContentItem = new List<int>(tagsForContentItem);
            IEnumerable<TagsContentItems> currentTagsForContentItem = _tagsContentItemsRepository.Fetch(x => x.ContentItemId == contentItemId);

            foreach (var tagContentItem in currentTagsForContentItem) {
                if (!newTagsForContentItem.Contains(tagContentItem.TagId)) {
                    _authorizationService.CheckAccess(Permissions.ApplyTag, _orchardServices.WorkContext.CurrentUser, null);

                    _tagsContentItemsRepository.Delete(tagContentItem);
                }
                else {
                    newTagsForContentItem.Remove(tagContentItem.TagId);
                }
            }

            foreach (var newTagForContentItem in newTagsForContentItem) {
                _authorizationService.CheckAccess(Permissions.ApplyTag, _orchardServices.WorkContext.CurrentUser, null);

                _tagsContentItemsRepository.Create(new TagsContentItems { ContentItemId = contentItemId, TagId = newTagForContentItem });
            }
        }
    }
}
