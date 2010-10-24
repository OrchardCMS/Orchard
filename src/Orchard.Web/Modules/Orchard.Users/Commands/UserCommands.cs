using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Users.Services;
using System.Web.Security;

namespace Orchard.Users.Commands {
    public class UserCommands : DefaultOrchardCommandHandler {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;

        public UserCommands(
            IContentManager contentManager,
            IMembershipService membershipService,
            IUserService userService) {
            _contentManager = contentManager;
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
            else
                return T("The authentication provider returned an error").ToString();
        }

        int MinPasswordLength {
            get {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }
    }
}