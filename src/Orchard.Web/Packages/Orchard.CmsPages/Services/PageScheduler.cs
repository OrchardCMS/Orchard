using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.CmsPages.Models;
using Orchard.Data;
using Orchard.Services;

namespace Orchard.CmsPages.Services {
    public interface IPageScheduler : IDependency {
        void AddPublishTask(PageRevision revision, DateTime moment);
        void Sweep();
        void ClearTasks(Page page);
    }

    [UsedImplicitly]
    public class PageScheduler : IPageScheduler {
        private readonly IClock _clock;
        private readonly IPageManager _pageManager;
        private readonly IRepository<Scheduled> _scheduledRepository;

        public PageScheduler(IClock clock, IPageManager pageManager, IRepository<Scheduled> scheduledRepository) {
            _clock = clock;
            _pageManager = pageManager;
            _scheduledRepository = scheduledRepository;
        }

        public void AddPublishTask(PageRevision revision, DateTime moment) {
            var task = new Scheduled {
                Action = ScheduledAction.Publish,
                Page = revision.Page,
                PageRevision = revision,
                ScheduledDate = moment,
            };
            task.Page.Scheduled.Add(task);
            task.PageRevision.Scheduled.Add(task);
        }

        public void Sweep() {
            var utcNow = _clock.UtcNow;

            var taskIds = GetTaskIdsToExecute(utcNow);
            var failedTaskIds = new List<int>();

            foreach (var taskId in taskIds) {
                //todo: transaction at this point

                try {
                    var task = _scheduledRepository.Get(taskId);
                    Process(task);
                    task.Page.Scheduled.Remove(task);
                    task.PageRevision.Scheduled.Remove(task);
                }
                catch {
                    //TODO: log failed task
                    failedTaskIds.Add(taskId);
                }
            }

            foreach (var failedTaskId in failedTaskIds) {
                //todo: transaction at this point

                try {
                    var task = _scheduledRepository.Get(failedTaskId);
                    _scheduledRepository.Delete(task);
                    task.Page.Scheduled.Remove(task);
                    task.PageRevision.Scheduled.Remove(task);
                }
                catch {  
                    //TODO: log critical, error removing failed task
                }
            }
        }

        public void ClearTasks(Page page) {
#warning UNIT TEST!!!!
            var tasks = _scheduledRepository.Fetch(x => x.Page == page);
            foreach(var task in tasks) {
                task.Page.Scheduled.Remove(task);
                task.PageRevision.Scheduled.Remove(task);
                task.Page = null;
                task.PageRevision = null;
            }
        }

        private int[] GetTaskIdsToExecute(DateTime scheduledBeforeUtc) {
            return _scheduledRepository.Fetch(
                x => x.ScheduledDate < scheduledBeforeUtc,
                o => o.Asc(x => x.ScheduledDate),
                0, 10)
                .Select(x => x.Id)
                .ToArray();
        }

        private void Process(Scheduled scheduled) {
            switch (scheduled.Action) {
                case ScheduledAction.Publish: {
                        ProcessPublish(scheduled);
                        break;
                    }
                case ScheduledAction.Unpublish: {
                        ProcessUnpublish(scheduled);
                        break;
                    }
                default: {
                        throw new ApplicationException("Unknown scheduled action " + scheduled.Action);
                    }
            }
        }

        
        private void ProcessPublish(Scheduled scheduled) {
            _pageManager.Publish(scheduled.PageRevision, new PublishOptions());
        }

        private void ProcessUnpublish(Scheduled scheduled) {
            throw new NotImplementedException();
        }

       
    }
}

