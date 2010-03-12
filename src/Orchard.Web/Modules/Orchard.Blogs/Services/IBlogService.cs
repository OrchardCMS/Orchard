using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public interface IBlogService : IDependency {
        Blog Get(string slug);
        IEnumerable<Blog> Get();
        void Create(Blog blog);
        void Edit(Blog blog);
        void Delete(Blog blog);
    }
}