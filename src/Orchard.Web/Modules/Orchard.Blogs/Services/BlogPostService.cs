using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Controllers;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Tasks.Scheduling;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogArchiveRecord> _blogArchiveRepository;
        private readonly IPublishingTaskManager _publishingTaskManager;

        public BlogPostService(IContentManager contentManager, IRepository<BlogArchiveRecord> blogArchiveRepository, IPublishingTaskManager publishingTaskManager) {
            _contentManager = contentManager;
            _blogArchiveRepository = blogArchiveRepository;
            _publishingTaskManager = publishingTaskManager;
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
            _publishingTaskManager.DeleteTasks(blogPost.ContentItem);
            _contentManager.Remove(blogPost.ContentItem);
        }

        public void Publish(BlogPost blogPost) {
            _publishingTaskManager.DeleteTasks(blogPost.ContentItem);
            _contentManager.Publish(blogPost.ContentItem);
        }

        public void Publish(BlogPost blogPost, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(blogPost.ContentItem, scheduledPublishUtc);
        }

        public void Unpublish(BlogPost blogPost) {
            _contentManager.Unpublish(blogPost.ContentItem);
        }

        public DateTime? GetScheduledPublishUtc(BlogPost blogPost) {
            var task = _publishingTaskManager.GetPublishTask(blogPost.ContentItem);
            return (task == null ? null : task.ScheduledUtc);
        }

        private IContentQuery<ContentItem, CommonRecord> GetBlogQuery(ContentPart<BlogRecord> blog, VersionOptions versionOptions) {
            return
                _contentManager.Query(versionOptions, BlogPostDriver.ContentType.Name).Join<CommonRecord>().Where(
                    cr => cr.Container == blog.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}