using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.JobsQueue.Models;
using Orchard.Settings;

namespace Orchard.JobsQueue.Services {
    public class JobsQueueManager : IJobsQueueManager {
        private readonly IRepository<QueuedJobRecord> _jobRepository;
        private readonly JobsQueueSettingsPart _jobsQueueSettingsPart;

        public JobsQueueManager(
            IRepository<QueuedJobRecord> jobRepository, 
            ISiteService siteService) {
                _jobRepository = jobRepository;
            _jobsQueueSettingsPart = siteService.GetSiteSettings().As<JobsQueueSettingsPart>();
        }

        public void Resume() {
            _jobsQueueSettingsPart.Status = JobsQueueStatus.Idle;
        }

        public void Pause() {
            _jobsQueueSettingsPart.Status = JobsQueueStatus.Paused;
        }

        public int GetJobsCount() {
            return GetMessagesQuery().Count();
        }

        public IEnumerable<QueuedJobRecord> GetJobs(int startIndex, int pageSize) {
            return GetMessagesQuery()
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();
        }

        public QueuedJobRecord GetJob(int id) {
            return _jobRepository.Get(id);
        }

        private IQueryable<QueuedJobRecord> GetMessagesQuery() {
            var query = _jobRepository
                .Table
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.CreatedUtc);

            return query;
        }

        public void Delete(QueuedJobRecord job) {
            _jobRepository.Delete(job);
        }
    }
}