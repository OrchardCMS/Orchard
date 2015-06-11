using System;
using System.Collections.Generic;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Jobs;
using Orchard.Azure.MediaServices.Services.Tasks;
using Orchard;

namespace Orchard.Azure.MediaServices.Services.Jobs {
    public interface IJobManager : IDependency {
        IEnumerable<Job> GetJobsFor(CloudVideoPart part);
        Job GetJobById(int id);
        Job CreateJobFor(CloudVideoPart part, Action<Job> initialize = null);
        void DeleteJobsFor(CloudVideoPart part);
        void DeleteJobs(IEnumerable<Job> jobs);
        IEnumerable<Job> GetOpenJobs();
        IEnumerable<Job> GetActiveJobs();
        Task CreateTaskFor(Job job, Action<Task> initialize = null);
        ITaskProvider GetTaskProviderByName(string name);
        void CloseJobsFor(CloudVideoPart part);
    }
}
