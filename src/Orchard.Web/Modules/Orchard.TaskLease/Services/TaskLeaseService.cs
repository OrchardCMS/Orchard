using System;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Services;
using Orchard.TaskLease.Models;

namespace Orchard.TaskLease.Services {

    /// <summary>
    /// Provides a database driven implementation of <see cref="ITaskLeaseService" />
    /// </summary>
    [Obsolete("Use Orchard.Tasks.Locking.DistributedLockService instead.")]
    public class TaskLeaseService : ITaskLeaseService {

        private readonly IRepository<TaskLeaseRecord> _repository;
        private readonly IClock _clock;
        private readonly IApplicationEnvironment _applicationEnvironment;

        public TaskLeaseService(
            IRepository<TaskLeaseRecord> repository,
            IClock clock,
            IApplicationEnvironment applicationEnvironment) {

            _repository = repository;
            _clock = clock;
            _applicationEnvironment = applicationEnvironment;
        }

        public string Acquire(string taskName, DateTime expiredUtc) {
            var environmentIdentifier = _applicationEnvironment.GetEnvironmentIdentifier();

            // retrieve current lease for the specified task
            var taskLease = _repository.Get(x => x.TaskName == taskName);

            // create a new lease if there is no current lease for this task
            if (taskLease == null) {
                taskLease = new TaskLeaseRecord {
                    TaskName = taskName,
                    MachineName = environmentIdentifier,
                    State = String.Empty,
                    UpdatedUtc = _clock.UtcNow,
                    ExpiredUtc = expiredUtc
                };

                _repository.Create(taskLease);
                _repository.Flush();

                return String.Empty;
            }

            // lease can't be aquired only if for a different machine and it has not expired
            if (taskLease.MachineName != environmentIdentifier && taskLease.ExpiredUtc >= _clock.UtcNow) {
                return null;
            }

            // otherwise update information
            taskLease.MachineName = environmentIdentifier;
            taskLease.UpdatedUtc = _clock.UtcNow;
            taskLease.ExpiredUtc = expiredUtc;

            _repository.Flush();

            return taskLease.State;
        }

        public void Update(string taskName, string state) {
            var environmentIdentifier = _applicationEnvironment.GetEnvironmentIdentifier();

            // retrieve current lease for the specified task
            var taskLease = _repository.Get(x => x.TaskName == taskName && x.MachineName == environmentIdentifier);

            if (taskLease == null) {
                return;
            }

            taskLease.State = state;
            _repository.Flush();
        }

        public void Update(string taskName, string state, DateTime expiredUtc) {
            var environmentIdentifier = _applicationEnvironment.GetEnvironmentIdentifier();

            // retrieve current lease for the specified task
            var taskLease = _repository.Get(x => x.TaskName == taskName && x.MachineName == environmentIdentifier);

            if (taskLease == null) {
                return;
            }

            taskLease.ExpiredUtc = expiredUtc;
            taskLease.State = state;

            _repository.Flush();
        }
    }
}