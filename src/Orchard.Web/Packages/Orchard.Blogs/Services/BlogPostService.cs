using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;

        public BlogPostService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public BlogPost Get(Blog blog, string slug) {
            return Get(blog, slug, VersionOptions.Published);
        }

        public BlogPost Get(Blog blog, string slug, VersionOptions versionOptions) {
            return
                _contentManager.Query<BlogPost, BlogPostRecord>(versionOptions).Join<RoutableRecord>().Where(rr => rr.Slug == slug).
                    Join<CommonRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord).List().
                    SingleOrDefault();
        }

        public IEnumerable<BlogPost> Get(Blog blog) {
            return Get(blog, VersionOptions.Published);
        }

        public IEnumerable<BlogPost> Get(Blog blog, VersionOptions versionOptions) {
            return GetBlogQuery(blog, versionOptions).List();
        }

        public IEnumerable<BlogPost> Get(Blog blog, ArchiveData archiveData) {
            var query = GetBlogQuery(blog, VersionOptions.Published);

            if (archiveData.Day > 0)
                query =
                    query.Where(
                        cr =>
                        cr.CreatedUtc >= new DateTime(archiveData.Year, archiveData.Month, archiveData.Day) &&
                        cr.CreatedUtc < new DateTime(archiveData.Year, archiveData.Month, archiveData.Day + 1));
            else if (archiveData.Month > 0)
                query =
                    query.Where(
                        cr =>
                        cr.CreatedUtc >= new DateTime(archiveData.Year, archiveData.Month, 1) &&
                        cr.CreatedUtc < new DateTime(archiveData.Year, archiveData.Month + 1, 1));
            else
                query =
                    query.Where(
                        cr =>
                        cr.CreatedUtc >= new DateTime(archiveData.Year, 1, 1) &&
                        cr.CreatedUtc < new DateTime(archiveData.Year + 1, 1, 1));

            return query.List();
        }

        public void Delete(BlogPost blogPost) {
            _contentManager.Remove(blogPost.ContentItem);
        }

        public void Publish(BlogPost blogPost) {
            _contentManager.Publish(blogPost.ContentItem);
            blogPost.Published = DateTime.UtcNow;
        }

        public void Unpublish(BlogPost blogPost) {
            _contentManager.Unpublish(blogPost.ContentItem);
            blogPost.Published = null;
        }

        private IContentQuery<BlogPost, CommonRecord> GetBlogQuery(ContentPart<BlogRecord> blog, VersionOptions versionOptions) {
            return
                _contentManager.Query<BlogPost, BlogPostRecord>(versionOptions).Join<CommonRecord>().Where(
                    cr => cr.Container == blog.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}