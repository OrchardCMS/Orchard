using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Blogs.Services
{
    public interface IBlogService : IDependency {
        BlogPart Get(string path);
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<BlogPart> Get();
        IEnumerable<BlogPart> Get(VersionOptions versionOptions);
        void Delete(ContentItem blog);
        void ProcessBlogPostsCount(int blogPartId);
        IEnumerable<ContentTypeDefinition> GetBlogPostContentTypes();
    }
}