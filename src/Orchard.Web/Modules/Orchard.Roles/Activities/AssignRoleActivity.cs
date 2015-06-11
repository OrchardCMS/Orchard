using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Activities {
    [OrchardFeature("Orchard.Roles.Workflows")]
    public class AssignRoleActivity : Task {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<UserRolesPartRecord> _repository;
        private readonly IRoleService _roleService;

        public AssignRoleActivity(
            IWorkContextAccessor workContextAccessor,
            IRepository<UserRolesPartRecord> repository,
            IRoleService roleService) {
            _workContextAccessor = workContextAccessor;
            _repository = repository;
            _roleService = roleService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public override string Name {
            get { return "AssignRole"; }
        }

        public override LocalizedString Category {
            get { return T("User"); }
        }

        public override LocalizedString Description {
            get { return T("Assign specific roles to the current content item if it's a user.");  }
        }

        public override string Form {
            get { return "SelectRoles"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {T("Done")};
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var user = workflowContext.Content.As<IUserRoles>();

            // if the current workflow subject is not a user, use current user
            if (user == null) {
                user = _workContextAccessor.GetContext().CurrentUser.As<IUserRoles>();
            }
            
            var roles = GetRoles(activityContext);

            if (user != null) {
                foreach (var role in roles) {
                    if (!user.Roles.Contains(role)) {
                        var roleRecord = _roleService.GetRoleByName(role);
                        if (roleRecord != null) {
                            _repository.Create(new UserRolesPartRecord {UserId = user.Id, Role = roleRecord});
                        }
                        else {
                            Logger.Debug("Role not found: {0}", role);
                        }
                    }
                }
            }

            yield return T("Done");
        }
        
        private IEnumerable<string> GetRoles(ActivityContext context) {
            var roles = context.GetState<string>("Roles");

            if (String.IsNullOrEmpty(roles)) {
                return Enumerable.Empty<string>();
            }

            return roles.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}