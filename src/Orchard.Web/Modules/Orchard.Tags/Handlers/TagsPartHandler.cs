using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Tags.Models;

namespace Orchard.Tags.Handlers {
    [UsedImplicitly]
    public class TagsPartHandler : ContentHandler {
        public TagsPartHandler(IRepository<TagsPartRecord> repository, IRepository<TagRecord> tagsRepository, IRepository<ContentTagRecord> tagsContentItemsRepository) {
            Filters.Add(StorageFilter.For(repository));
 
            OnLoading<TagsPart>((context, tags) => {
                // populate list of attached tags on demand
                tags._currentTags.Loader(list => {
                    foreach(var tag in tagsContentItemsRepository.Fetch(x => x.TagsPartRecord.Id == context.ContentItem.Id))
                        list.Add(tag.TagRecord);
                    return list;
                });

            });

            OnRemoved<TagsPart>((context, tags) => {
                tagsContentItemsRepository.Flush();

                var tagsPart = context.ContentItem.As<TagsPart>();

                // delete orphan tags (for each tag, if there is no other contentItem than the one being deleted, it's an orphan)
                foreach ( var tag in tagsPart.CurrentTags ) {
                    if ( tagsContentItemsRepository.Fetch(x => x.TagsPartRecord.Id != context.ContentItem.Id).Count() == 0 ) {
                        tagsRepository.Delete(tag);
                    }
                }

                // delete tag links with this contentItem (tagsContentItems)
                foreach ( var tagsContentItem in tagsContentItemsRepository.Fetch(x => x.TagsPartRecord.Id == context.ContentItem.Id) ) {
                    tagsContentItemsRepository.Delete(tagsContentItem);
                }

            });

            OnIndexing<TagsPart>((context, tagsPart) => context.DocumentIndex
                                                    .Add("tags", String.Join(", ", tagsPart.CurrentTags.Select(t => t.TagName))).Analyze());
        }
    }
}