using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public interface IBlogService : IDependency {
        Blog Get(string slug);
        IEnumerable<Blog> Get();
        void Delete(Blog blog);
    }
}