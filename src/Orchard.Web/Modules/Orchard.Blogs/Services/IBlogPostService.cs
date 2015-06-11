using System;
using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public interface IBlogPostService : IDependency {
        BlogPostPart Get(int id);
        BlogPostPart Get(int id, VersionOptions versionOptions);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart, VersionOptions versionOptions);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart, ArchiveData archiveData);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart, int skip, int count);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart, int skip, int count, VersionOptions versionOptions);
        int PostCount(BlogPart blogPart);
        int PostCount(BlogPart blogPart, VersionOptions versionOptions);
        IEnumerable<KeyValuePair<ArchiveData, int>> GetArchives(BlogPart blogPart);
        void Delete(BlogPostPart blogPostPart);
        void Publish(BlogPostPart blogPostPart);
        void Publish(BlogPostPart blogPostPart, DateTime scheduledPublishUtc);
        void Unpublish(BlogPostPart blogPostPart);
        DateTime? GetScheduledPublishUtc(BlogPostPart blogPostPart);

    }
}