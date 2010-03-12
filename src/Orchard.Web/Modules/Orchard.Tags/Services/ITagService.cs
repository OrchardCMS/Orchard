using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Tags.Models;

namespace Orchard.Tags.Services {
    public interface ITagService : IDependency {
        IEnumerable<Tag> GetTags();
        Tag GetTag(int id);
        Tag GetTagByName(string tagName);
        void CreateTag(string tagName);
        void DeleteTag(int id);
        void UpdateTag(int id, string tagName);
        IEnumerable<IContent> GetTaggedContentItems(int id);
        void TagContentItem(int contentItemId, string tagName);
        void UpdateTagsForContentItem(int contentItemId, IEnumerable<string> tagNamesForContentItem);
    }
}