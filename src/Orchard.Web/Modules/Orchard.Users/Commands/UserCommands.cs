using Orchard.Commands;
using Orchard.Security;
using Orchard.Users.Services;

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
            if (!_userService.VerifyUserUnicity(UserName, Email)) {
                Context.Output.WriteLine(T("User with that username and/or email already exists."));
                return;
            }

            if (Password == null || Password.Length < MinPasswordLength) {
                Context.Output.WriteLine(T("You must specify a password of {0} or more characters.", MinPasswordLength));
                return;
            }

            var user = _membershipService.CreateUser(new CreateUserParams(UserName, Password, Email, null, null, true));
            if (user == null) {
                Context.Output.WriteLine(T("Could not create user {0}. The authentication provider returned an error", UserName));
                return;
            }

            Context.Output.WriteLine(T("User created successfully"));
        }

        int MinPasswordLength {
            get {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }
    }
}