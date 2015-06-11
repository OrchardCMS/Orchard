using System.Collections.Generic;
using Orchard.Tags.Models;

namespace Orchard.Tags.Services {
    public interface ITagCloudService : IDependency {
        IEnumerable<TagCount> GetPopularTags(int buckets, string slug);
    }
}