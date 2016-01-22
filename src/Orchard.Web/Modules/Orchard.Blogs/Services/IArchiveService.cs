using Orchard.Blogs.Models;

namespace Orchard.Blogs.Services {
    public interface IArchiveService : IDependency {
        void RebuildArchive(BlogPart blog);
    }
}