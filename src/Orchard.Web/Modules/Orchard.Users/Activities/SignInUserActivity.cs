using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Users.Activities {
    [OrchardFeature("Orchard.Users.Workflows")]
    public class SignInUserActivity : Task {
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;

        public SignInUserActivity(IMembershipService membershipService, IAuthenticationService authenticationService, IUserEventHandler userEventHandler) {
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "SignInUser"; }
        }

        public override LocalizedString Category {
            get { return T("User"); }
        }

        public override LocalizedString Description {
            get { return T("Signs in a user based on the specified credentials, or if the current content item is a user, that user is signed in."); }
        }

        public override string Form {
            get { return "SignInUser"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("IncorrectUserNameOrPassword"),
                T("Done")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var userNameOrEmail = activityContext.GetState<string>("UserNameOrEmail");
            var password = activityContext.GetState<string>("Password");
            var createPersistentCookie = IsTrueish(activityContext.GetState<string>("CreatePersistentCookie"));
            var user = workflowContext.Content != null ? workflowContext.Content.As<IUser>() : default(IUser);

            if (user == null) {
                if (String.IsNullOrWhiteSpace(userNameOrEmail) || String.IsNullOrWhiteSpace(password)) {
                    yield return T("IncorrectUserNameOrPassword");
                    yield break;
                }

                user = _membershipService.ValidateUser(userNameOrEmail, password);
            }

            if (user == null) {
                yield return T("IncorrectUserNameOrPassword");
                yield break;
            }

            _userEventHandler.LoggingIn(userNameOrEmail, password);
            _authenticationService.SignIn(user, createPersistentCookie);
            _userEventHandler.LoggedIn(user);            

            yield return T("Done");
        }

        private bool IsTrueish(string value) {
            if (String.IsNullOrWhiteSpace(value))
                return false;

            var falseValues = new[] {"false", "off", "no"};
            return falseValues.All(x => !String.Equals(x, value, StringComparison.OrdinalIgnoreCase));
        }
    }
}