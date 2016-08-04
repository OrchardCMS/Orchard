using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Orchard.Environment;
using Orchard.Events;
using Orchard.JobsQueue.Models;
using Orchard.Logging;
using Orchard.Tasks.Locking.Services;

namespace Orchard.JobsQueue.Services {
    public class JobsQueueProcessor : IJobsQueueProcessor {
        private readonly Work<IJobsQueueManager> _jobsQueueManager;
        private readonly Work<IEventBus> _eventBus;
        private readonly Work<IDistributedLockService> _distributedLockService;

        public JobsQueueProcessor(
            Work<IJobsQueueManager> jobsQueueManager,
            Work<IEventBus> eventBus,
            Work<IDistributedLockService> distributedLockService) {

            _jobsQueueManager = jobsQueueManager;
            _eventBus = eventBus;
            _distributedLockService = distributedLockService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void ProcessQueue() {
            IDistributedLock @lock;
            if (_distributedLockService.Value.TryAcquireLock(GetType().FullName, TimeSpan.FromMinutes(5), out @lock)) {
                using (@lock) {
                    IEnumerable<QueuedJobRecord> messages;

                    while ((messages = _jobsQueueManager.Value.GetJobs(0, 10).ToArray()).Any()) {
                        foreach (var message in messages) {
                            ProcessMessage(message);
                        }
                    }
                }
            }
        }

        private void ProcessMessage(QueuedJobRecord job) {

            Logger.Debug("Processing job {0}.", job.Id);

            try {
                var payload = JObject.Parse(job.Parameters);
                var parameters = payload.ToDictionary();

                _eventBus.Value.Notify(job.Message, parameters);

                Logger.Debug("Processed job Id {0}.", job.Id);
            }
            catch (Exception e) {
                Logger.Error(e, "An unexpected error while processing job {0}. Error message: {1}.", job.Id, e);
            }
            finally {
                _jobsQueueManager.Value.Delete(job);
            }
        }
    }

    public static class JObjectExtensions {

        public static IDictionary<string, object> ToDictionary(this JObject jObject) {
            return (IDictionary<string, object>)Convert(jObject);
        }

        private static object Convert(this JToken jToken) {
            if (jToken == null) {
                throw new ArgumentNullException();
            }

            switch (jToken.Type) {
                case JTokenType.Array:
                    var array = jToken as JArray;
                    return array.Values().Select(Convert).ToArray();
                case JTokenType.Object:
                    var obj = jToken as JObject;
                    return obj
                        .Properties()
                        .ToDictionary(property => property.Name, property => Convert(property.Value));
                default:
                    return jToken.ToObject<object>();
            }
        }
    }
}
