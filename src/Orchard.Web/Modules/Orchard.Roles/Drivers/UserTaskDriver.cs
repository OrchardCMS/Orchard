using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Roles.Activities;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Drivers {
    [OrchardFeature("Orchard.Roles.Workflows")]
    public class UserTaskDriver : ContentPartDriver<ContentPart> {
        private readonly IWorkflowManager _workflowManager;
        private readonly IRepository<AwaitingActivityRecord> _awaitingActivityRepository;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserTaskDriver(
            IOrchardServices services,
            IWorkflowManager workflowManager,
            IRepository<AwaitingActivityRecord> awaitingActivityRepository,
            IWorkContextAccessor workContextAccessor,
            IHttpContextAccessor httpContextAccessor
            ) {
            _workflowManager = workflowManager;
            _awaitingActivityRepository = awaitingActivityRepository;
            _workContextAccessor = workContextAccessor;
            _httpContextAccessor = httpContextAccessor;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "UserTaskDriver"; }
        }

        protected override DriverResult Editor(ContentPart part, dynamic shapeHelper) {
            var results = new List<DriverResult> {
                ContentShape("UserTask_ActionButton", () => {
                    if (part.ContentItem.Record == null) {
                        return null;
                    }

                    var workContext = _workContextAccessor.GetContext();
                    var user = workContext.CurrentUser;

                    var awaiting = _awaitingActivityRepository.Table.Where(x => x.WorkflowRecord.ContentItemRecord == part.ContentItem.Record && x.ActivityRecord.Name == "UserTask").ToList();
                    var actions = awaiting.Where(x => UserIsInRole(x, user)).SelectMany(ListAction).ToList();

                    return shapeHelper.UserTask_ActionButton().Actions(actions);
                })
            };

            return Combined(results.ToArray());
        }

        // returns all the actions associated with a specific state
        private static IEnumerable<string> ListAction(AwaitingActivityRecord x) {
            var state = FormParametersHelper.FromJsonString(x.ActivityRecord.State);
            string actionState = state.Actions ?? "";
            return actionState.Split(',').Select(action => action.Trim());
        }

        // whether a user is in an accepted role for this state
        private static bool UserIsInRole(AwaitingActivityRecord x, IUser user) {
            var state = FormParametersHelper.FromJsonString(x.ActivityRecord.State);
            string rolesState = state.Roles ?? "";

            // "Any" if string is empty
            if (string.IsNullOrWhiteSpace(rolesState)) {
                return true;
            }
            var roles = rolesState.Split(',').Select(role => role.Trim());
            return UserTaskActivity.UserIsInRole(user, roles);
        }

        protected override DriverResult Editor(ContentPart part, IUpdateModel updater, dynamic shapeHelper) {
            var httpContext = _httpContextAccessor.Current();
            var name = httpContext.Request.Form["submit.Save"];
            if (!string.IsNullOrEmpty(name) && name.StartsWith("usertask-")) {
                name = name.Substring("usertask-".Length);

                var user = Services.WorkContext.CurrentUser;

                var awaiting = _awaitingActivityRepository.Table.Where(x => x.WorkflowRecord.ContentItemRecord == part.ContentItem.Record && x.ActivityRecord.Name == "UserTask").ToList();
                var actions = awaiting.Where(x => UserIsInRole(x, user)).SelectMany(ListAction).ToList();

                if (!actions.Contains(name)) {
                    Services.Notifier.Error(T("Not authorized to trigger {0}.", name));
                }
                else {
                    _workflowManager.TriggerEvent("UserTask", part, () => new Dictionary<string, object> { { "Content", part.ContentItem}, { "UserTask.Action", name } });
                }
            }

            return Editor(part, shapeHelper);
        }
    }
}