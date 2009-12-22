using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Tags.Models {
    [UsedImplicitly]
    public class HasTagsHandler : ContentHandler {
        private readonly IRepository<Tag> _tagsRepository;
        private readonly IRepository<TagsContentItems> _tagsContentItemsRepository;

        public HasTagsHandler(IRepository<Tag> tagsRepository, IRepository<TagsContentItems> tagsContentItemsRepository) {
            _tagsRepository = tagsRepository;
            _tagsContentItemsRepository = tagsContentItemsRepository;
            Filters.Add(new ActivatingFilter<HasTags>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasTags>("blogpost"));
        }


        protected override void Loading(LoadContentContext context) {
            if (context.ContentItem.Has<HasTags>() == false) {
                return;
            }

            HasTags tags = context.ContentItem.Get<HasTags>();
            tags.AllTags = _tagsRepository.Table.ToList();
            IEnumerable<TagsContentItems> tagsContentItems = _tagsContentItemsRepository.Fetch(x => x.ContentItemId == context.ContentItem.Id);
            foreach (var tagContentItem in tagsContentItems) {
                Tag tag = _tagsRepository.Get(tagContentItem.TagId);
                tags.CurrentTags.Add(tag);
            }
        }
    }
}
