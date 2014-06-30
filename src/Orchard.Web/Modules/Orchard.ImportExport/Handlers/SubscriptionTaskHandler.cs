using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Tasks.Scheduling;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class SubscriptionTaskHandler : IScheduledTaskHandler {
        private readonly IRecurringScheduledTaskManager _taskManager;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAuthenticationService _authenticationService;

        public const string TaskType = "RecurringTask.Subscription";

        public SubscriptionTaskHandler(IRecurringScheduledTaskManager taskManager,
            ISubscriptionService subscriptionService,
            IAuthenticationService authenticationService) {
            _taskManager = taskManager;
            _subscriptionService = subscriptionService;
            _authenticationService = authenticationService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            //each step of the import process is executed in its own transaction
            if (context.Task.TaskType == TaskType && context.Task.ContentItem != null) {
                var subscription = context.Task.ContentItem.As<DeploymentSubscriptionPart>();
                string executionId = null;
                try {
                    //By default there is no current user in the workcontext for background tasks.
                    //Set to the owner of the subscription.
                    _authenticationService.SetAuthenticatedUserForRequest(subscription.As<CommonPart>().Owner);
                    //recipe event handlers update run status as the recipe executes
                    executionId = _subscriptionService.RunSubscriptionTask(subscription.Id);
                }
                catch (Exception e) {
                    //Scheduling of import has failed.
                    Logger.Error(e, e.Message);
                    if (!string.IsNullOrEmpty(executionId)) {
                        _taskManager.SetTaskCompleted(executionId, RunStatus.Fail);
                    }
                }
            }
        }
    }
}
