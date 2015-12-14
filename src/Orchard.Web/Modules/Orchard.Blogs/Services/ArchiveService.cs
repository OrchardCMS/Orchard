using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Blogs.Services {
    public class ArchiveService : IArchiveService {
        private readonly IRepository<BlogPartArchiveRecord> _blogArchiveRepository;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ArchiveService(
            IRepository<BlogPartArchiveRecord> blogArchiveRepository,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor) {
            _blogArchiveRepository = blogArchiveRepository;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        public void RebuildArchive(BlogPart blogPart) {

            var first = _contentManager.Query<BlogPostPart>().Where<CommonPartRecord>(bp => bp.Container.Id == blogPart.Id).OrderBy<CommonPartRecord>(x => x.CreatedUtc).Slice(0, 1).FirstOrDefault();

            if (first == null) {
                return;
            }

            var last = _contentManager.Query<BlogPostPart>().Where<CommonPartRecord>(bp => bp.Container.Id == blogPart.Id).OrderByDescending<CommonPartRecord>(x => x.CreatedUtc).Slice(0, 1).FirstOrDefault();

            DateTime? start = DateTime.MaxValue;
            if (first.As<CommonPart>() != null) {
                start = first.As<CommonPart>().CreatedUtc;
            }

            DateTime? end = DateTime.MinValue;
            if (last.As<CommonPart>() != null) {
                end = last.As<CommonPart>().CreatedUtc;
            }

            // delete previous archive records
            foreach (var record in _blogArchiveRepository.Table.Where(x => x.BlogPart.Id == blogPart.Id)) {
                _blogArchiveRepository.Delete(record);
            }

            if (!start.HasValue || !end.HasValue) {
                return;
            }

            // get the time zone for the current request
            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;

            // build a collection of all the post dates
            var blogPostDates = new List<DateTime>();
            var blogPosts = _contentManager.Query<BlogPostPart>().Where<CommonPartRecord>(bp => bp.Container.Id == blogPart.Id);
            foreach (var blogPost in blogPosts.List()) {
                if (blogPost.As<CommonPart>() != null)
                    if (blogPost.As<CommonPart>().CreatedUtc.HasValue) {
                        DateTime timeZoneAdjustedCreatedDate = TimeZoneInfo.ConvertTimeFromUtc(blogPost.As<CommonPart>().CreatedUtc.Value, timeZone);
                        blogPostDates.Add(timeZoneAdjustedCreatedDate);
                    }
            }

            for (int year = start.Value.Year; year <= end.Value.Year; year++) {
                for (int month = 1; month <= 12; month++) {
                    var fromDateUtc = new DateTime(year, month, 1);
                    var from = TimeZoneInfo.ConvertTimeFromUtc(fromDateUtc, timeZone);
                    var to = TimeZoneInfo.ConvertTimeFromUtc(fromDateUtc.AddMonths(1), timeZone);

                    // skip the first months of the first year until a month has posts
                    //  for instance, if started posting in May 2000, don't write counts for Jan 200 > April 2000... start May 2000
                    if (from < TimeZoneInfo.ConvertTimeFromUtc(new DateTime(start.Value.Year, start.Value.Month, 1), timeZone))
                        continue;
                    // skip the last months of the last year if no posts
                    //  for instance, no need to have archives for months in the future
                    if (to > end.Value.AddMonths(1))
                        continue;

                    //var count = _contentManager.Query<BlogPostPart>().Where<CommonPartRecord>(x => x.CreatedUtc.Value >= from && x.CreatedUtc.Value < to).Count();
                    var count = blogPostDates.Count(bp => bp >= @from && bp < to);

                    var newArchiveRecord = new BlogPartArchiveRecord { BlogPart = blogPart.ContentItem.Record, Year = year, Month = month, PostCount = count };
                    _blogArchiveRepository.Create(newArchiveRecord);
                }
            }
        }
    }
}