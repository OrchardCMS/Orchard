using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Tags.Models;

namespace Orchard.Tags.Handlers {
    [UsedImplicitly]
    public class HasTagsHandler : ContentHandler {
        public HasTagsHandler(IRepository<Tag> tagsRepository, IRepository<TagsContentItems> tagsContentItemsRepository) {
            Filters.Add(new ActivatingFilter<HasTags>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasTags>("blogpost"));
            Filters.Add(new ActivatingFilter<HasTags>("page"));

            OnLoading<HasTags>((context, tags) => {

                // provide names of all tags on demand
                tags._allTags.Loader(list => tagsRepository.Table.ToList());

                // populate list of attached tags on demand
                tags._currentTags.Loader(list => {
                    var tagsContentItems = tagsContentItemsRepository.Fetch(x => x.ContentItemId == context.ContentItem.Id);
                    foreach (var tagContentItem in tagsContentItems) {
                        var tag = tagsRepository.Get(tagContentItem.TagId);
                        list.Add(tag);
                    }
                    return list;
                });

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