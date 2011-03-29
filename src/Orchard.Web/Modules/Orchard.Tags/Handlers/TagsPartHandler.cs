using System;
using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Tags.Models;
using Orchard.Tags.Services;

namespace Orchard.Tags.Handlers {
    public class TagsPartHandler : ContentHandler {
        public TagsPartHandler(IRepository<TagsPartRecord> repository, ITagService tagService) {
            Filters.Add(StorageFilter.For(repository));
 
            OnRemoved<TagsPart>((context, tags) => 
                tagService.RemoveTagsForContentItem(context.ContentItem));

            OnIndexing<TagsPart>(
                (context, tagsPart) => {
                    foreach (var tag in tagsPart.CurrentTags) {
                        context.DocumentIndex.Add("tags", tag.TagName).Analyze();
                    }
                });
        }
    }
}