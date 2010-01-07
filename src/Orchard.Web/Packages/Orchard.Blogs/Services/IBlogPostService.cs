using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public interface IBlogPostService : IDependency {
        BlogPost Get(Blog blog, string slug);
        IEnumerable<BlogPost> Get(Blog blog);
        IEnumerable<BlogPost> Get(Blog blog, ArchiveData archiveData);
        void Delete(BlogPost blogPost);
    }
}