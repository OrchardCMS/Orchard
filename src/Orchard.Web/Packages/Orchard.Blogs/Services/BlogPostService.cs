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
                _contentManager.Query(versionOptions, "blogpost").Join<RoutableRecord>().Where(rr => rr.Slug == slug).
                    Join<CommonRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord).List().
                    SingleOrDefault().As<BlogPost>();
        }

        public IEnumerable<BlogPost> Get(Blog blog) {
            return Get(blog, VersionOptions.Published);
        }

        public IEnumerable<BlogPost> Get(Blog blog, VersionOptions versionOptions) {
            return GetBlogQuery(blog, versionOptions).List().Select(ci => ci.As<BlogPost>());
        }

        public IEnumerable<BlogPost> Get(Blog blog, ArchiveData archiveData) {
            var query = GetBlogQuery(blog, VersionOptions.Published);

            if (archiveData.Day > 0) {
                var dayDate = new DateTime(archiveData.Year, archiveData.Month, archiveData.Day);

                query = query.Where(cr => cr.CreatedUtc >= dayDate && cr.CreatedUtc < dayDate.AddDays(1));
            }
            else if (archiveData.Month > 0)
            {
                var monthDate = new DateTime(archiveData.Year, archiveData.Month, 1);

                query = query.Where(cr => cr.CreatedUtc >= monthDate && cr.CreatedUtc < monthDate.AddMonths(1));
            }
            else {
                var yearDate = new DateTime(archiveData.Year, 1, 1);

                query = query.Where(cr => cr.CreatedUtc >= yearDate && cr.CreatedUtc < yearDate.AddYears(1));
            }

            return query.List().Select(ci => ci.As<BlogPost>());
        }

        public IEnumerable<KeyValuePair<ArchiveData, int>> GetArchives(Blog blog) {
            return new List<KeyValuePair<ArchiveData, int>> {
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2010/1"), 5),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/12"), 23),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/11"), 4),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/9"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/8"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/7"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/6"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/5"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/4"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/3"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/2"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2009/1"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/12"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/11"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/10"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/9"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/7"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/6"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/5"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/4"), 1),
                                                                new KeyValuePair<ArchiveData, int>(new ArchiveData("2008/3"), 1)
                                                            };
        }

        public void Delete(BlogPost blogPost) {
            _contentManager.Remove(blogPost.ContentItem);
        }

        public void Publish(BlogPost blogPost) {
            _contentManager.Publish(blogPost.ContentItem);
            //TODO: (erikpo) Not sure if this is needed or not
            blogPost.Published = DateTime.UtcNow;
        }

        public void Publish(BlogPost blogPost, DateTime publishDate) {
            //TODO: (erikpo) This logic should move out of blogs and pages and into content manager
            if (blogPost.Published != null && blogPost.Published.Value >= DateTime.UtcNow)
                _contentManager.Unpublish(blogPost.ContentItem);
            blogPost.Published = publishDate;
        }

        public void Unpublish(BlogPost blogPost) {
            _contentManager.Unpublish(blogPost.ContentItem);
            //TODO: (erikpo) Not sure if this is needed or not
            blogPost.Published = null;
        }

        private IContentQuery<ContentItem, CommonRecord> GetBlogQuery(ContentPart<BlogRecord> blog, VersionOptions versionOptions) {
            return
                _contentManager.Query(versionOptions, "blogpost").Join<CommonRecord>().Where(
                    cr => cr.Container == blog.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}