using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Users;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.UI.Notify;
using Orchard.Users.Events;

namespace Orchard.Users.Services {
    public interface IApproveUserService : IDependency {
        void Approve(UserPart contentItem);
        void Disable(UserPart contentItem);
    }

    public class ApproveUserService : IApproveUserService {

        private readonly IUserEventHandler _userEventHandlers;
        private readonly IOrchardServices _orchardServices;
        
        public ApproveUserService(
           IUserEventHandler userEventHandlers,
           IOrchardServices orchardServices) {

            _userEventHandlers = userEventHandlers;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }


        public void Approve(UserPart part) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users"))) {
                return;
            }

            if (part == null) { 
                return;
            }

            part.RegistrationStatus = UserStatus.Approved;
            _orchardServices.Notifier.Information(T("User {0} approved", part.UserName));
            _userEventHandlers.Approved(part);
        }

        public void Disable(UserPart part) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return;

            if (part == null) {
                return;
            }

            part.RegistrationStatus = UserStatus.Pending;
            _orchardServices.Notifier.Information(T("User {0} disabled", part.UserName));
            _userEventHandlers.Moderate(part);
        }
    }
}