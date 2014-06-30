using System.Collections.Generic;
using Orchard.ImportExport.Models;
using Orchard.Tasks.Scheduling;

namespace Orchard.ImportExport.Services {
    public interface IRecurringScheduledTaskManager : IDependency {
        ScheduledTaskRunHistory GetLastTaskRun(int id, RunStatus? runStatus);
        IScheduledTask GetNextScheduledTask(int id);
        IList<ScheduledTaskRunHistory> GetHistory(int? id, int limit);
        void ScheduleTaskForNextRun(RecurringTaskPart part, bool runNow);
        ScheduledTaskRunHistory GetTaskRunByExecutionId(string executionId);
        void UpdateTaskRunStatus(string executionId, RunStatus status);
        ScheduledTaskRunHistory SetTaskStarted(RecurringTaskPart taskPart, string executionId);
        void SetTaskCompleted(string executionId, RunStatus status);
        void ClearHistory(double daysToRetain);
    }
}