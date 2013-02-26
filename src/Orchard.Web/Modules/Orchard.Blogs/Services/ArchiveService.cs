using System;
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

            var first = _contentManager.Query<BlogPostPart>().OrderBy<CommonPartRecord>(x => x.CreatedUtc).Slice(0, 1).FirstOrDefault();

            if (first == null) {
                return;
            }

            var last = _contentManager.Query<BlogPostPart>().OrderByDescending<CommonPartRecord>(x => x.CreatedUtc).Slice(0, 1).FirstOrDefault();

            DateTime? start = DateTime.MaxValue;
            if (first.As<CommonPart>() != null) {
                start = first.As<CommonPart>().CreatedUtc;
            }
            
            DateTime? end = DateTime.MinValue;
            if (last.As<CommonPart>() != null) {
                end = first.As<CommonPart>().CreatedUtc;
            }
            
            // delete previous archive records
            foreach (var record in _blogArchiveRepository.Table.Where(x => x.BlogPart == blogPart.Record)) {
                _blogArchiveRepository.Delete(record);
            }

            if (!start.HasValue || !end.HasValue) {
                return;
            }
            
            // get the time zone for the current request
            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;

            for (int year = start.Value.Year - 1; year <= end.Value.Year + 1; year++) {
                for (int month = 1; month <= 12; month++) {
                    int yearCopy = year;
                    int monthCopy = month;
                    var from = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(yearCopy, monthCopy, 1), timeZone);
                    var to = from.AddMonths(1);
                    var count = _contentManager.Query<BlogPostPart>().Where<CommonPartRecord>(x => x.CreatedUtc.Value >= from && x.CreatedUtc.Value < to).Count();

                    var newArchiveRecord = new BlogPartArchiveRecord { BlogPart = blogPart.Record, Year = from.Year, Month = from.Month, PostCount = count };
                    _blogArchiveRepository.Create(newArchiveRecord);
                }
            }
        }
    }
}