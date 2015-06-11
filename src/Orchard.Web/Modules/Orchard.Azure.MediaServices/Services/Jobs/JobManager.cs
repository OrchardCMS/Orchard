using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Jobs;
using Orchard.Azure.MediaServices.Models.Records;
using Orchard.Azure.MediaServices.Services.Tasks;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;

namespace Orchard.Azure.MediaServices.Services.Jobs {
    public class JobManager : Component, IJobManager {
        private readonly IRepository<JobRecord> _jobRepository;
        private readonly IRepository<TaskRecord> _taskRepository;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ITaskProvider> _taskProviders;

        public JobManager(
            IRepository<JobRecord> jobRepository, 
            IRepository<TaskRecord> taskRepository, 
            IContentManager contentManager, 
            IEnumerable<ITaskProvider> taskProviders) {

            _jobRepository = jobRepository;
            _taskRepository = taskRepository;
            _contentManager = contentManager;
            _taskProviders = taskProviders;
        }

        public IEnumerable<Job> GetJobsFor(CloudVideoPart part) {
            Logger.Debug("GetJobsFor() invoked for cloud video item with ID {0}.", part.Id);

            var jobsQuery =
                from jobRecord in GetJobRecordsFor(part)
                select Activate(jobRecord); // OK to call Activate() inline here because GetJobRecordsFor() contains an inner projection.

            return jobsQuery.ToArray();
        }

        public Job GetJobById(int id) {
            Logger.Debug("GetJob() invoked with id value {0}.", id);

            return Activate(_jobRepository.Get(id));
        }

        public Job CreateJobFor(CloudVideoPart part, Action<Job> initialize = null) {
            Logger.Debug("CreateJobFor() invoked for cloud video item with ID {0}.", part.Id);

            var newJob = Activate(new JobRecord {
                CloudVideoPartId = part.Id
            });

            if (initialize != null)
                initialize(newJob);

            _jobRepository.Create(newJob.Record);
            Logger.Information("New job was created with record ID {0} for cloud video item with ID {1}.", newJob.Record.Id, part.Id);

            return newJob;
        }

        public void DeleteJobsFor(CloudVideoPart part) {
            Logger.Debug("DeleteJobsFor() invoked for cloud video item with ID {0}.", part.Id);

            var records = GetJobRecordsFor(part);

            foreach (var record in records) {
                _jobRepository.Delete(record);
                Logger.Information("Job with record ID {0} was deleted.", record.Id);
            }

            Logger.Information("Jobs were deleted for cloud video item with ID {0}.", part.Id);
        }

        public void DeleteJobs(IEnumerable<Job> jobs) {
            Logger.Debug("DeleteJobs() invoked.");

            foreach (var job in jobs) {
                _jobRepository.Delete(job.Record);
                Logger.Information("Job with record ID {0} was deleted.", job.Record.Id);
            }
        }

        public Task CreateTaskFor(Job job, Action<Task> initialize = null) {
            Logger.Debug("CreateTaskFor() invoked for job with record ID {0}.", job.Record.Id);

            var task = Activate(new TaskRecord {
                Job = job.Record
            });

            if (initialize != null)
                initialize(task);

            job.Record.Tasks.Add(task.Record);
            _taskRepository.Create(task.Record);
            Logger.Information("Task was created for job with record ID {0}.", job.Record.Id);
            
            return task;
       }

        public ITaskProvider GetTaskProviderByName(string name) {
            Logger.Debug("GetTaskProviderByName() invoked with name value '{0}'.", name);

            return _taskProviders.SingleOrDefault(x => x.Name == name);
        }

        public void CloseJobsFor(CloudVideoPart part) {
            var openJobs = GetJobRecordsFor(part).Select(Activate).Where(x => x.IsOpen);

            foreach (var job in openJobs) {
                job.Status = JobStatus.Archived;
            }
        }

        public IEnumerable<Job> GetOpenJobs() {
            Logger.Debug("GetOpenJobs() invoked.");

            var openJobsQuery =
                from jobRecord in _jobRepository.Table
                select jobRecord;

            return openJobsQuery.Select(Activate).Where(x => x.IsOpen); // Can't call Activate() inline here because NHibernate will try to cache it!
        }

        public IEnumerable<Job> GetActiveJobs() {
            Logger.Debug("GetActiveJobs() invoked.");

            var openJobsQuery =
                from job in
                    (from jobRecord in _jobRepository.Table
                     select jobRecord).Select(Activate).ToArray() // Can't call Activate() inline here because NHibernate will try to cache it!
                where job.IsActive
                select job;

            return openJobsQuery.ToArray();
        }

        private IQueryable<JobRecord> GetJobRecordsFor(CloudVideoPart part) {
            return
                from jobRecord in _jobRepository.Table
                where jobRecord.CloudVideoPartId == part.Id
                select jobRecord;
        }

        private Job Activate(JobRecord record) {
            if (record == null)
                return null;

            var job = new Job(record);
            job._tasksField.Loader(() => record.Tasks.Select(Activate));
            job._cloudVideoPartField.Loader(() => _contentManager.Get<CloudVideoPart>(record.CloudVideoPartId, VersionOptions.Latest));
            return job;
        }

        private Task Activate(TaskRecord record) {
            var task = new Task {
                Record = record
            };

            return task;
        }
    }
}