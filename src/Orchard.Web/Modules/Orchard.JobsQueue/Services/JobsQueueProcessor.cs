using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Orchard.Environment;
using Orchard.Events;
using Orchard.Logging;
using Orchard.JobsQueue.Models;
using Orchard.Services;
using Orchard.TaskLease.Services;

namespace Orchard.JobsQueue.Services {
    public class JobsQueueProcessor : IJobsQueueProcessor {
        private readonly Work<IJobsQueueManager> _jobsQueueManager;
        private readonly Work<IClock> _clock;
        private readonly Work<ITaskLeaseService> _taskLeaseService;
        private readonly IEventBus _eventBus;
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();

        public JobsQueueProcessor(
            Work<IClock> clock,
            Work<IJobsQueueManager> jobsQueueManager,
            Work<ITaskLeaseService> taskLeaseService,
            IEventBus eventBus) {
            _clock = clock;
            _jobsQueueManager = jobsQueueManager;
            _taskLeaseService = taskLeaseService;
            _eventBus = eventBus;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public void ProcessQueue() {
            // prevent two threads on the same machine to process the message queue
            if (_rwl.TryEnterWriteLock(0)) {
                try {
                    _taskLeaseService.Value.Acquire("JobsQueueProcessor", _clock.Value.UtcNow.AddMinutes(5));
                    IEnumerable<QueuedJobRecord> messages;

                    while ((messages = _jobsQueueManager.Value.GetJobs(0, 10).ToArray()).Any()) {
                        foreach (var message in messages) {
                            ProcessMessage(message);
                        }
                    }
                }
                finally {
                    _rwl.ExitWriteLock();
                }
            }
        }

        private void ProcessMessage(QueuedJobRecord job) {

            Logger.Debug("Processing job {0}.", job.Id);

            try {
                var payload = JObject.Parse(job.Parameters);
                var parameters = payload.ToDictionary();

                _eventBus.Notify(job.Message, parameters);

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