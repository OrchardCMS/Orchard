using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;

namespace Orchard.Users.Drivers {

    [OrchardFeature("Orchard.Users.PasswordEditor")]
    public class UserPartPasswordDriver : ContentPartDriver<UserPart> {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandler;

        public Localizer T { get; set; }

        public UserPartPasswordDriver(
            MembershipService membershipService,
            IUserService userService,
            IUserEventHandler userEventHandler) {

            _membershipService = membershipService;
            _userService = userService;
            _userEventHandler = userEventHandler;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(UserPart part, dynamic shapeHelper) {
            return ContentShape("Parts_User_EditPassword_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/User.EditPassword",
                    Model: new UserEditPasswordViewModel { User = part },
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(UserPart part, IUpdateModel updater, dynamic shapeHelper) {
            var editModel = new UserEditPasswordViewModel { User = part };
            var canUpdatePassword = true;
            if (updater != null) {
                if (updater.TryUpdateModel(editModel, Prefix, null, null)) {
                    if (!(string.IsNullOrEmpty(editModel.Password) && string.IsNullOrEmpty(editModel.ConfirmPassword))) {
                        if (string.IsNullOrEmpty(editModel.Password) || string.IsNullOrEmpty(editModel.ConfirmPassword)) {
                            updater.AddModelError("MissingPassword", T("Password or Confirm Password field is empty."));
                            canUpdatePassword = false;
                        } else {
                            if (editModel.Password != editModel.ConfirmPassword) {
                                updater.AddModelError("ConfirmPassword", T("Password confirmation must match."));
                                canUpdatePassword = false;
                            }                            
                        }
                        IDictionary<string, LocalizedString> validationErrors;
                        if (!_userService.PasswordMeetsPolicies(editModel.Password, part, out validationErrors)) {
                            updater.AddModelErrors(validationErrors);
                            canUpdatePassword = false;
                        }
                        if (canUpdatePassword) {
                            var actUser = _membershipService.GetUser(part.UserName);
                            // I need to store current password in a variable to save it in the PasswordHistoryRepository.
                            _userEventHandler.ChangingPassword(actUser, editModel.Password);                         
                            _membershipService.SetPassword(actUser, editModel.Password);
                            _userEventHandler.ChangedPassword(actUser, editModel.Password);
                        }
                    }
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}