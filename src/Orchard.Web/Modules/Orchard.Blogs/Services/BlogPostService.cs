using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Tasks.Scheduling;

namespace Orchard.Blogs.Services {
    public class BlogPostService : IBlogPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<BlogPartArchiveRecord> _blogArchiveRepository;
        private readonly IPublishingTaskManager _publishingTaskManager;

        public BlogPostService(
            IContentManager contentManager, 
            IRepository<BlogPartArchiveRecord> blogArchiveRepository, 
            IPublishingTaskManager publishingTaskManager) {
            _contentManager = contentManager;
            _blogArchiveRepository = blogArchiveRepository;
            _publishingTaskManager = publishingTaskManager;
        }

        public BlogPostPart Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public BlogPostPart Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get<BlogPostPart>(id, versionOptions);
        }

        public IEnumerable<BlogPostPart> Get(BlogPart blogPart) {
            return Get(blogPart, VersionOptions.Published);
        }

        public IEnumerable<BlogPostPart> Get(BlogPart blogPart, VersionOptions versionOptions) {
            return GetBlogQuery(blogPart, versionOptions).List().Select(ci => ci.As<BlogPostPart>());
        }

        public IEnumerable<BlogPostPart> Get(BlogPart blogPart, int skip, int count) {
            return Get(blogPart, skip, count, VersionOptions.Published);
        }

        public IEnumerable<BlogPostPart> Get(BlogPart blogPart, int skip, int count, VersionOptions versionOptions) {
            return GetBlogQuery(blogPart, versionOptions)
                    .Slice(skip, count)
                    .ToList()
                    .Select(ci => ci.As<BlogPostPart>());
        }

        public int PostCount(BlogPart blogPart) {
            return PostCount(blogPart, VersionOptions.Published);
        }

        public int PostCount(BlogPart blogPart, VersionOptions versionOptions) {
            return _contentManager.Query(versionOptions, "BlogPost")
                .Join<CommonPartRecord>().Where(
                    cr => cr.Container.Id == blogPart.Id)
                .Count();
        }

        public IEnumerable<BlogPostPart> Get(BlogPart blogPart, ArchiveData archiveData) {
            var query = GetBlogQuery(blogPart, VersionOptions.Published);

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

            return query.List().Select(ci => ci.As<BlogPostPart>());
        }

        public IEnumerable<KeyValuePair<ArchiveData, int>> GetArchives(BlogPart blogPart) {
            var query = 
                from bar in _blogArchiveRepository.Table
                where bar.BlogPart.Id == blogPart.Id
                orderby bar.Year descending, bar.Month descending
                select bar;

            return
                query.ToList().Select(
                    bar =>
                    new KeyValuePair<ArchiveData, int>(new ArchiveData(string.Format("{0}/{1}", bar.Year, bar.Month)),
                                                       bar.PostCount));
        }

        public void Delete(BlogPostPart blogPostPart) {
            _publishingTaskManager.DeleteTasks(blogPostPart.ContentItem);
            _contentManager.Remove(blogPostPart.ContentItem);
        }

        public void Publish(BlogPostPart blogPostPart) {
            _publishingTaskManager.DeleteTasks(blogPostPart.ContentItem);
            _contentManager.Publish(blogPostPart.ContentItem);
        }

        public void Publish(BlogPostPart blogPostPart, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(blogPostPart.ContentItem, scheduledPublishUtc);
        }

        public void Unpublish(BlogPostPart blogPostPart) {
            _contentManager.Unpublish(blogPostPart.ContentItem);
        }

        public DateTime? GetScheduledPublishUtc(BlogPostPart blogPostPart) {
            var task = _publishingTaskManager.GetPublishTask(blogPostPart.ContentItem);
            return (task == null ? null : task.ScheduledUtc);
        }

        private IContentQuery<ContentItem, CommonPartRecord> GetBlogQuery(BlogPart blog, VersionOptions versionOptions) {
            return
                _contentManager.Query(versionOptions, "BlogPost")
                .Join<CommonPartRecord>().Where(
                    cr => cr.Container.Id == blog.Id).OrderByDescending(cr => cr.CreatedUtc)
                    ;
        }
    }
}