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
        public string Create() {
            string userUnicityMessage = _userService.VerifyUserUnicity(UserName, Email);
            if (userUnicityMessage != null) {
                return userUnicityMessage;
            }
            if (Password == null || Password.Length < MinPasswordLength) {
                return T("You must specify a password of {0} or more characters.", MinPasswordLength).ToString();
            }

            var user = _membershipService.CreateUser(new CreateUserParams(UserName, Password, Email, null, null, true));
            if (user != null)
                return T("User created successfully").ToString();
            
            return T("The authentication provider returned an error").ToString();
        }

        int MinPasswordLength {
            get {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }
    }
}