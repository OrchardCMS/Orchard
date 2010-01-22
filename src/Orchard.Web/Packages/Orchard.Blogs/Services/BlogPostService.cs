using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Controllers;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;
using Orchard.Data;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogArchiveRecord> _blogArchiveRepository;

        public BlogPostService(IContentManager contentManager, IRepository<BlogArchiveRecord> blogArchiveRepository) {
            _contentManager = contentManager;
            _blogArchiveRepository = blogArchiveRepository;
        }

        public BlogPost Get(Blog blog, string slug) {
            return Get(blog, slug, VersionOptions.Published);
        }

        public BlogPost Get(Blog blog, string slug, VersionOptions versionOptions) {
            return
                _contentManager.Query(versionOptions, BlogPostDriver.ContentType.Name).Join<RoutableRecord>().Where(rr => rr.Slug == slug).
                    Join<CommonRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord).List().
                    SingleOrDefault().As<BlogPost>();
        }

        public BlogPost Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public BlogPost Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get<BlogPost>(id, versionOptions);
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
            var query = 
                from bar in _blogArchiveRepository.Table
                where bar.Blog == blog.Record
                orderby bar.Year descending, bar.Month descending
                select bar;

            return
                query.ToList().Select(
                    bar =>
                    new KeyValuePair<ArchiveData, int>(new ArchiveData(string.Format("{0}/{1}", bar.Year, bar.Month)),
                                                       bar.PostCount));
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
                _contentManager.Query(versionOptions, BlogPostDriver.ContentType.Name).Join<CommonRecord>().Where(
                    cr => cr.Container == blog.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}