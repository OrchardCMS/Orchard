using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Tags.Models {
    [UsedImplicitly]
    public class HasTagsHandler : ContentHandler {
        public HasTagsHandler(IRepository<Tag> tagsRepository, IRepository<TagsContentItems> tagsContentItemsRepository) {
            Filters.Add(new ActivatingFilter<HasTags>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasTags>("blogpost"));

            OnLoading<HasTags>((context, ht) => {
                HasTags tags = context.ContentItem.As<HasTags>();
                tags.AllTags = tagsRepository.Table.ToList();
                IEnumerable<TagsContentItems> tagsContentItems = tagsContentItemsRepository.Fetch(x => x.ContentItemId == context.ContentItem.Id);
                foreach (var tagContentItem in tagsContentItems) {
                    Tag tag = tagsRepository.Get(tagContentItem.TagId);
                    tags.CurrentTags.Add(tag);
                }
            });

            OnRemoved<HasTags>((context, ht) => {
                tagsContentItemsRepository.Flush();

                HasTags tags = context.ContentItem.As<HasTags>();
                foreach (var tag in tags.CurrentTags) {
                    if (!tagsContentItemsRepository.Fetch(x => x.ContentItemId == context.ContentItem.Id).Any()) {
                        tagsRepository.Delete(tag);
                    }
                }
            });
        }
    }
}
