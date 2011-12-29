using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public interface IBlogService : IDependency {
        BlogPart Get(int id);
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<BlogPart> Get();
        IEnumerable<BlogPart> Get(VersionOptions versionOptions);
        void Delete(ContentItem blog);
    }
}