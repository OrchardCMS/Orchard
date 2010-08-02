using System;
using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public interface IBlogPostService : IDependency {
        BlogPostPart Get(BlogPart blogPart, string slug);
        BlogPostPart Get(BlogPart blogPart, string slug, VersionOptions versionOptions);
        BlogPostPart Get(int id);
        BlogPostPart Get(int id, VersionOptions versionOptions);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart, VersionOptions versionOptions);
        IEnumerable<BlogPostPart> Get(BlogPart blogPart, ArchiveData archiveData);
        IEnumerable<KeyValuePair<ArchiveData, int>> GetArchives(BlogPart blogPart);
        void Delete(BlogPostPart blogPostPart);
        void Publish(BlogPostPart blogPostPart);
        void Publish(BlogPostPart blogPostPart, DateTime scheduledPublishUtc);
        void Unpublish(BlogPostPart blogPostPart);
        DateTime? GetScheduledPublishUtc(BlogPostPart blogPostPart);
    }
}