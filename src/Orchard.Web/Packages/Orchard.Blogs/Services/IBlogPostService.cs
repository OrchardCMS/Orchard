using System;
using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public interface IBlogPostService : IDependency {
        BlogPost Get(Blog blog, string slug);
        BlogPost Get(Blog blog, string slug, VersionOptions versionOptions);
        IEnumerable<BlogPost> Get(Blog blog);
        IEnumerable<BlogPost> Get(Blog blog, VersionOptions versionOptions);
        IEnumerable<BlogPost> Get(Blog blog, ArchiveData archiveData);
        void Delete(BlogPost blogPost);
        void Publish(BlogPost blogPost);
        void Publish(BlogPost blogPost, DateTime publishDate);
        void Unpublish(BlogPost blogPost);
    }
}