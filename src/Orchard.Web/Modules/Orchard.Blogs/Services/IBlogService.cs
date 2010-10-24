using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public interface IBlogService : IDependency {
        BlogPart Get(string slug);
        IEnumerable<BlogPart> Get();
        void Delete(BlogPart blogPart);
    }
}