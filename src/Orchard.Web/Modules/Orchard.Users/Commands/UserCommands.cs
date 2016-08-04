using Orchard.Commands;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Services;
using System.Collections.Generic;

namespace Orchard.Users.Commands {
    public class UserCommands : DefaultOrchardCommandHandler {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;

        public UserCommands(
            IMembershipService membershipService,
            IUserService userService) {
            _membershipService = membershipService;
            _userService = userService;
        }

        [OrchardSwitch]
        public string UserName { get; set; }

        [OrchardSwitch]
        public string Password { get; set; }

        [OrchardSwitch]
        public string Email { get; set; }

        [OrchardSwitch]
        public string FileName { get; set; }

        [CommandName("user create")]
        [CommandHelp("user create /UserName:<username> /Password:<password> /Email:<email>\r\n\t" + "Creates a new User")]
        [OrchardSwitches("UserName,Password,Email")]
        public void Create() {
	        if (string.IsNullOrWhiteSpace(UserName)) {
		        Context.Output.WriteLine(T("Username cannot be empty."));
		        return;
	        }

            if (!_userService.VerifyUserUnicity(UserName, Email)) {
                Context.Output.WriteLine(T("User with that username and/or email already exists."));
                return;
            }

            IDictionary<string, LocalizedString> validationErrors;
            if (!_userService.PasswordMeetsPolicies(Password, out validationErrors)) {
                foreach (var error in validationErrors) {
                    Context.Output.WriteLine(error.Value);
                }
                return;
            }

            var user = _membershipService.CreateUser(new CreateUserParams(UserName, Password, Email, null, null, true));
            if (user == null) {
                Context.Output.WriteLine(T("Could not create user {0}. The authentication provider returned an error", UserName));
                return;
            }

            Context.Output.WriteLine(T("User created successfully"));
        }
    }
}