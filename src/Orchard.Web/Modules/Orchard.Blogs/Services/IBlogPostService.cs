using System;
using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public interface IBlogPostService : IDependency {
        BlogPost Get(Blog blog, string slug);
        BlogPost Get(Blog blog, string slug, VersionOptions versionOptions);
        BlogPost Get(int id);
        BlogPost Get(int id, VersionOptions versionOptions);
        IEnumerable<BlogPost> Get(Blog blog);
        IEnumerable<BlogPost> Get(Blog blog, VersionOptions versionOptions);
        IEnumerable<BlogPost> Get(Blog blog, ArchiveData archiveData);
        IEnumerable<KeyValuePair<ArchiveData, int>> GetArchives(Blog blog);
        void Delete(BlogPost blogPost);
        void Publish(BlogPost blogPost);
        void Publish(BlogPost blogPost, DateTime scheduledPublishUtc);
        void Unpublish(BlogPost blogPost);
        DateTime? GetScheduledPublishUtc(BlogPost blogPost);
    }
}