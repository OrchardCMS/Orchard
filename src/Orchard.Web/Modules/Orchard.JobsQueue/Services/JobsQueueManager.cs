using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.JobsQueue.Models;
using Orchard.Settings;

namespace Orchard.JobsQueue.Services {
    public class JobsQueueManager : IJobsQueueManager {
        private readonly IRepository<QueuedJobRecord> _jobRepository;
        private readonly Lazy<JobsQueueSettingsPart> _jobsQueueSettingsPart;

        public JobsQueueManager(
            IRepository<QueuedJobRecord> jobRepository, 
            ISiteService siteService) {
                _jobRepository = jobRepository;
                _jobsQueueSettingsPart = new Lazy<JobsQueueSettingsPart>(() => { return siteService.GetSiteSettings().As<JobsQueueSettingsPart>(); });
        }

        public void Resume() {
            _jobsQueueSettingsPart.Value.Status = JobsQueueStatus.Idle;
        }

        public void Pause() {
            _jobsQueueSettingsPart.Value.Status = JobsQueueStatus.Paused;
        }

        public int GetJobsCount() {
            return _jobRepository
                .Table
                .Count();
        }

        public IEnumerable<QueuedJobRecord> GetJobs(int startIndex, int pageSize) {
            IQueryable<QueuedJobRecord> query = _jobRepository
                .Table
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.CreatedUtc);

            if(startIndex > 0) {
                query = query.Skip(startIndex);
            }
                
            query = query.Take(pageSize);

            return query.ToList();
        }

        public QueuedJobRecord GetJob(int id) {
            return _jobRepository.Get(id);
        }

        public void Delete(QueuedJobRecord job) {
            _jobRepository.Delete(job);
        }
    }
}