using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Users.Activities {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class CreateUserActivity : Task {
        private readonly IUserService _userService;
        private readonly IMembershipService _membershipService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CreateUserActivity(IUserService userService, IMembershipService membershipService, IContentDefinitionManager contentDefinitionManager) {
            _userService = userService;
            _membershipService = membershipService;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "CreateUser"; }
        }

        public override LocalizedString Category {
            get { return T("User"); }
        }

        public override LocalizedString Description {
            get { return T("Creates a new User based on the specified values."); }
        }

        public override string Form {
            get { return "CreateUser"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("InvalidUserNameOrEmail"),
                T("InvalidPassword"),
                T("UserNameOrEmailNotUnique"),
                T("WrongConfiguration"),
                T("Done")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var userName = activityContext.GetState<string>("UserName");
            var email = activityContext.GetState<string>("Email");
            var password = activityContext.GetState<string>("Password");
            var approved = activityContext.GetState<bool>("Approved");
            var contentType = activityContext.GetState<string>("ContentType");
            

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(email)) {
                yield return T("InvalidUserNameOrEmail");
                yield break;
            }

            if (String.IsNullOrWhiteSpace(password)) {
                yield return T("InvalidPassword");
                yield break;
            }

            if (!String.IsNullOrWhiteSpace(contentType) &&
                (_contentDefinitionManager.GetTypeDefinition(contentType) == null ||
                    !_contentDefinitionManager.GetTypeDefinition(contentType).Parts.Any(p => p.PartDefinition.Name == "UserPart"))) {
                yield return T("WrongConfiguration");
                yield break;
            }

            if (!_userService.VerifyUserUnicity(userName, email)) {
                yield return T("UserNameOrEmailNotUnique");
                yield break;
            }            

            userName = userName.Trim();

            var user = _membershipService.CreateUser(
                new CreateUserParams(
                    userName,
                    password,
                    email,
                    isApproved: approved,
                    passwordQuestion: null,
                    passwordAnswer: null,
                    contentType: contentType
                    ));

            workflowContext.Content = user;

            yield return T("Done");
        }
    }
}