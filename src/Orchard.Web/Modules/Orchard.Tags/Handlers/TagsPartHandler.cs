using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Tags.Models;

namespace Orchard.Tags.Handlers {
    [UsedImplicitly]
    public class TagsPartHandler : ContentHandler {
        public TagsPartHandler(IRepository<Tag> tagsRepository, IRepository<TagsContentItems> tagsContentItemsRepository) {
 
            OnLoading<TagsPart>((context, tags) => {

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

            OnRemoved<TagsPart>((context, tags) => {
                tagsContentItemsRepository.Flush();

                TagsPart tagsPart = context.ContentItem.As<TagsPart>();
                foreach (var tag in tagsPart.CurrentTags) {
                    if (!tagsContentItemsRepository.Fetch(x => x.ContentItemId == context.ContentItem.Id).Any()) {
                        tagsRepository.Delete(tag);
                    }
                }
            });
        }
    }
}