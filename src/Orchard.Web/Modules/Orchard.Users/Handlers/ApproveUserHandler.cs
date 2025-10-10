using Orchard.ContentManagement.Handlers;
using Orchard.Mvc;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Users.Handlers {
    public class ApproveUserHandler : ContentHandler {
        private readonly IApproveUserService _approveUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApproveUserHandler(
           IApproveUserService approveUserService,
           IHttpContextAccessor httpContextAccessor) {
            _approveUserService = approveUserService;
            _httpContextAccessor = httpContextAccessor;

            OnPublished<UserPart>((context, part) => {
                var httpContext = _httpContextAccessor.Current();
                // verify user click correct button and 
                // registration status is correct to approve/disable
                if (httpContext.Request.Form["submit.Save"] == "submit.ApproveUser" &&
                    part.RegistrationStatus == UserStatus.Pending) {
                    _approveUserService.Approve(part);
                }

                if (httpContext.Request.Form["submit.Save"] == "submit.DisableUser" &&
                    part.RegistrationStatus == UserStatus.Approved) {
                    _approveUserService.Disable(part);
                }
            });
        }
    }
}