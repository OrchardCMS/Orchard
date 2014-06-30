using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.Services;
using Orchard.Tasks.Scheduling;

namespace Orchard.ImportExport.Services {
    [OrchardFeature("Orchard.Deployment")]
    public class RecurringScheduledTaskManager : IRecurringScheduledTaskManager {
        private readonly IClock _clock;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IContentManager _contentManager;
        private readonly IRepository<ScheduledTaskRunHistory> _runHistoryRepository;

        public RecurringScheduledTaskManager(IClock clock,
            IScheduledTaskManager scheduledTaskManager,
            IContentManager contentManager,
            IRepository<ScheduledTaskRunHistory> runHistoryRepository) {
            _clock = clock;
            _scheduledTaskManager = scheduledTaskManager;
            _contentManager = contentManager;
            _runHistoryRepository = runHistoryRepository;
        }

        public ScheduledTaskRunHistory GetLastTaskRun(int id, RunStatus? runStatus) {
            return _runHistoryRepository.Table.OrderByDescending(t => t.Id)
                .FirstOrDefault(t => t.ContentItemRecord.Id == id && (!runStatus.HasValue || t.RunStatus == runStatus));
        }

        public IScheduledTask GetNextScheduledTask(int id) {
            return _scheduledTaskManager.GetTasks(_contentManager.Get(id)).OrderByDescending(t => t.ScheduledUtc).FirstOrDefault();
        }

        public IList<ScheduledTaskRunHistory> GetHistory(int? id, int limit = 20) {
            return _runHistoryRepository.Table.OrderByDescending(c => c.Id).Take(limit).ToList();
        }

        public ScheduledTaskRunHistory SetTaskStarted(RecurringTaskPart taskPart, string executionId) {
            //create a new run history record
            var runHistory = new ScheduledTaskRunHistory {
                ContentItemRecord = taskPart != null ? taskPart.Record.ContentItemRecord : null,
                RunStartUtc = _clock.UtcNow,
                RunStatus = RunStatus.Started,
                ExecutionId = executionId
            };
            _runHistoryRepository.Create(runHistory);
            return runHistory;
        }

        public void SetTaskCompleted(string executionId, RunStatus status) {
            var task = GetTaskRunByExecutionId(executionId);
            if (task != null) {
                task.RunStatus = status;
                if (task.RunStatus == RunStatus.Success || task.RunStatus == RunStatus.Fail) {
                    task.RunCompletedUtc = _clock.UtcNow;
                }

                _runHistoryRepository.Update(task);

                var taskPart = task.ContentItemRecord != null ?
                    _contentManager.Get<RecurringTaskPart>(task.ContentItemRecord.Id) : null;

                //TODO: Add retry attempt logic e.g. retry 3 times on failure then suspend
                if (taskPart != null && taskPart.IsActive) {
                    ScheduleTaskForNextRun(taskPart, false);
                }
            }
        }

        public void ScheduleTaskForNextRun(RecurringTaskPart part, bool runNow) {
            ClearExistingTasks(part); //ensure there are no existing pending tasks
            var nextRun = runNow ? _clock.UtcNow : CalculateNextRunUtc(part);
            CreateNextTask(part.Settings["TaskType"], nextRun, part.ContentItem);
        }

        public void UpdateTaskRunStatus(string executionId, RunStatus status) {
            var task = GetTaskRunByExecutionId(executionId);
            if (task != null) {
                task.RunStatus = status;
                if (task.RunStatus == RunStatus.Success || task.RunStatus == RunStatus.Fail) {
                    task.RunCompletedUtc = _clock.UtcNow;
                }

                _runHistoryRepository.Update(task);
            }
        }

        public ScheduledTaskRunHistory GetTaskRunByExecutionId(string executionId) {
            return _runHistoryRepository.Table.FirstOrDefault(t => t.ExecutionId == executionId);
        }

        public void ClearHistory(double daysToRetain) {
            var forDeletion = _runHistoryRepository.Table.Where(r => r.RunStartUtc < _clock.UtcNow.AddDays(-1 * daysToRetain)).ToList();

            for (int i = forDeletion.Count - 1; i >= 0; i--) {
                _runHistoryRepository.Delete(forDeletion[i]);
            }
        }

        private DateTime CalculateNextRunUtc(RecurringTaskPart part) {
            if (part.RepeatFrequencyInMinutes > 0)
                return _clock.UtcNow.AddMinutes(part.RepeatFrequencyInMinutes);

            return _clock.UtcNow;
        }

        private void ClearExistingTasks(RecurringTaskPart part) {
            _scheduledTaskManager.DeleteTasks(part.ContentItem, t => t.TaskType == part.Settings["TaskType"]);
        }

        private void CreateNextTask(string taskType, DateTime date, ContentItem contentItem) {
            _scheduledTaskManager.CreateTask(taskType, date, contentItem);
        }
    }
}