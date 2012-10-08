using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPartArchiveHandler : ContentHandler {
        private readonly IWorkContextAccessor _workContextAccessor;
        // contains the creation time of a blog part before it has been changed
        private readonly Dictionary<BlogPostPart, DateTime> _previousCreatedUtc = new Dictionary<BlogPostPart,DateTime>();

        public BlogPartArchiveHandler(
            IRepository<BlogPartArchiveRecord> blogArchiveRepository, 
            IBlogPostService blogPostService,
            IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;

            OnVersioning<BlogPostPart>((context, bp1, bp2) => {
                var commonPart = bp1.As<CommonPart>();
                if(commonPart == null || !commonPart.CreatedUtc.HasValue || !bp1.IsPublished)
                    return;

                _previousCreatedUtc[bp2] = commonPart.CreatedUtc.Value;
            });

            OnPublished<BlogPostPart>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, bp));
            OnUnpublished<BlogPostPart>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, bp));
            OnRemoved<BlogPostPart>((context, bp) => RecalculateBlogArchive(blogArchiveRepository, bp));
        }

        private void RecalculateBlogArchive(IRepository<BlogPartArchiveRecord> blogArchiveRepository, BlogPostPart blogPostPart) {
            blogArchiveRepository.Flush();
            
            var commonPart = blogPostPart.As<CommonPart>();
                if(commonPart == null || !commonPart.CreatedUtc.HasValue)
                    return;

            // get the time zone for the current request
            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;

            var previousCreatedUtc = _previousCreatedUtc.ContainsKey(blogPostPart) ? _previousCreatedUtc[blogPostPart] : DateTime.MinValue;
            previousCreatedUtc = TimeZoneInfo.ConvertTimeFromUtc(previousCreatedUtc, timeZone);

            var previousMonth = previousCreatedUtc.Month;
            var previousYear = previousCreatedUtc.Year;

            var newCreatedUtc = commonPart.CreatedUtc;
            newCreatedUtc = newCreatedUtc.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(newCreatedUtc.Value, timeZone) : newCreatedUtc;

            var newMonth = newCreatedUtc.HasValue ? newCreatedUtc.Value.Month : 0;
            var newYear = newCreatedUtc.HasValue ? newCreatedUtc.Value.Year : 0;

            // if archives are the same there is nothing to do
            if (previousMonth == newMonth && previousYear == newYear) {
                return;
            }
            
            // decrement previous archive record
            var previousArchiveRecord = blogArchiveRepository.Table
                .Where(x => x.BlogPart == blogPostPart.BlogPart.Record
                    && x.Month == previousMonth
                    && x.Year == previousYear)
                .FirstOrDefault();

            if (previousArchiveRecord != null && previousArchiveRecord.PostCount > 0) {
                previousArchiveRecord.PostCount--;
            }

            // if previous count is now zero, delete the record
            if (previousArchiveRecord != null && previousArchiveRecord.PostCount == 0) {
                blogArchiveRepository.Delete(previousArchiveRecord);
            }
            
            // increment new archive record
            var newArchiveRecord = blogArchiveRepository.Table
                .Where(x => x.BlogPart == blogPostPart.BlogPart.Record
                    && x.Month == newMonth
                    && x.Year == newYear)
                .FirstOrDefault();

            // if record can't be found create it
            if (newArchiveRecord == null) {
                newArchiveRecord = new BlogPartArchiveRecord { BlogPart = blogPostPart.BlogPart.Record, Year = newYear, Month = newMonth, PostCount = 0 };
                blogArchiveRepository.Create(newArchiveRecord);
            }

            newArchiveRecord.PostCount++;            
        }
    }
}