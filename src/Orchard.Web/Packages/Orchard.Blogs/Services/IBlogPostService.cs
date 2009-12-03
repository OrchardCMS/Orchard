using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public interface IBlogPostService : IDependency {
        BlogPost Get(Blog blog, string slug);
        IEnumerable<BlogPost> Get(Blog blog);
        BlogPost Create(CreateBlogPostParams parameters);
        void Delete(BlogPost blogPost);
    }
}