using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    public class BlogPartArchiveHandler : ContentHandler {
        private readonly IRepository<BlogPartArchiveRecord> _blogArchiveRepository;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        // contains the creation time of a blog part before it has been changed
        private readonly Dictionary<int, DateTime> _previousCreatedUtc = new Dictionary<int, DateTime>();

        public BlogPartArchiveHandler(
            IRepository<BlogPartArchiveRecord> blogArchiveRepository,
            IBlogPostService blogPostService,
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager) {
            _blogArchiveRepository = blogArchiveRepository;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;

            OnUpdating<CommonPart>((context, cp) => { if (context.ContentItem.Has<BlogPostPart>()) SavePreviousCreatedDate(context.Id); });
            OnRemoving<BlogPostPart>((context, bp) => SavePreviousCreatedDate(context.Id));
            OnUnpublishing<BlogPostPart>((context, bp) => SavePreviousCreatedDate(context.Id));

            OnPublished<BlogPostPart>((context, bp) => ManageBlogArchiveSync(bp));
            OnUnpublished<BlogPostPart>((context, bp) => ReduceBlogArchive(bp));
            OnRemoved<BlogPostPart>((context, bp) => ReduceBlogArchive(bp));
        }

        private void SavePreviousCreatedDate(int contentItemId) {
            if (_previousCreatedUtc.ContainsKey(contentItemId)) {
                return;
            }

            var previousPublishedVersion = _contentManager.Get(contentItemId, VersionOptions.Published);

            // retrieve the creation date when it was published
            if (previousPublishedVersion != null) {
                var versionCommonPart = previousPublishedVersion.As<ICommonPart>();
                if (versionCommonPart.CreatedUtc.HasValue) {
                    _previousCreatedUtc[contentItemId] = versionCommonPart.CreatedUtc.Value;
                }
            }
        }

        private void ReduceBlogArchive(BlogPostPart blogPostPart) {
            _blogArchiveRepository.Flush();

            // don't reduce archive count if the content item is not published
            if (!_previousCreatedUtc.ContainsKey(blogPostPart.Id)) {
                return;
            }

            var commonPart = blogPostPart.As<ICommonPart>();
            if (commonPart == null || !commonPart.CreatedUtc.HasValue)
                return;

            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;
            var datetime = TimeZoneInfo.ConvertTimeFromUtc(commonPart.CreatedUtc.Value, timeZone);

            var previousArchiveRecord = _blogArchiveRepository.Table
                .FirstOrDefault(x => x.BlogPart.Id == blogPostPart.BlogPart.Id
                                    && x.Month == datetime.Month
                                    && x.Year == datetime.Year);

            if (previousArchiveRecord == null)
                return;

            if (previousArchiveRecord.PostCount > 1)
                previousArchiveRecord.PostCount--;
            else
                _blogArchiveRepository.Delete(previousArchiveRecord);

            blogPostPart.ArchiveSync = null;
        }

        private void ReduceBlogArchiveByDate(int BlogPartId, DateTime? dateBlogPost) {
            _blogArchiveRepository.Flush();

            if (BlogPartId!= 0 && dateBlogPost.HasValue) {
                var previousArchiveRecord = _blogArchiveRepository.Table
                    .FirstOrDefault(x => x.BlogPart.Id == BlogPartId
                                        && x.Month == dateBlogPost.Value.Month
                                        && x.Year == dateBlogPost.Value.Year);

                if (previousArchiveRecord == null)
                    return;

                if (previousArchiveRecord.PostCount > 1)
                    previousArchiveRecord.PostCount--;
                else
                    _blogArchiveRepository.Delete(previousArchiveRecord);
            }
            else {
                return;
            }

        }
     
        private void IncreaseBlogArchive(BlogPostPart blogPostPart) {
            _blogArchiveRepository.Flush();

            var commonPart = blogPostPart.As<ICommonPart>();
            if (commonPart == null || !commonPart.CreatedUtc.HasValue)
                return;

            // get the time zone for the current request
            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;

            var newCreatedUtc = commonPart.CreatedUtc;
            newCreatedUtc = TimeZoneInfo.ConvertTimeFromUtc(newCreatedUtc.Value, timeZone);

            var newMonth = newCreatedUtc.Value.Month;
            var newYear = newCreatedUtc.Value.Year;

            // increment new archive record
            var newArchiveRecord = _blogArchiveRepository
                .Table
                .FirstOrDefault(x => x.BlogPart.Id == blogPostPart.BlogPart.Id
                                     && x.Month == newMonth
                                     && x.Year == newYear);

            // if record can't be found create it
            if (newArchiveRecord == null) {
                newArchiveRecord = new BlogPartArchiveRecord { BlogPart = blogPostPart.BlogPart.ContentItem.Record, Year = newYear, Month = newMonth, PostCount = 0 };
                _blogArchiveRepository.Create(newArchiveRecord);
            }

            newArchiveRecord.PostCount++;
        }

        private void ManageBlogArchiveSync(BlogPostPart blogPostPart) {
            var commonPart = blogPostPart.As<ICommonPart>();
            if (commonPart == null || !commonPart.CreatedUtc.HasValue)
                return;

            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;
            var creationDate = TimeZoneInfo.ConvertTimeFromUtc(commonPart.CreatedUtc.Value, timeZone);

            if (blogPostPart.ArchiveSync == null) {
                IncreaseBlogArchive(blogPostPart);
                blogPostPart.ArchiveSync = creationDate;
            }
            else {
                if (creationDate == blogPostPart.ArchiveSync) {
                    return;
                }
                else {
                    ReduceBlogArchiveByDate(blogPostPart.BlogPart.Id, blogPostPart.ArchiveSync);
                    IncreaseBlogArchive(blogPostPart);
                    blogPostPart.ArchiveSync = creationDate;
                }
            }
        }
    }
}