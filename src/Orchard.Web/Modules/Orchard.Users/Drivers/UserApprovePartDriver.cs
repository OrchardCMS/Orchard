using Laser.Orchard.StartupConfig.ApproveUserExtension.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;
using Orchard.Users;
using Orchard;

namespace Laser.Orchard.StartupConfig.ApproveUserExtension.Drivers {
    public class UserApprovePartDriver : ContentPartDriver<UserPart> {
        private const string TemplateName = "Parts/User.Approve";
        private readonly IApproveUserService _approveUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardServices _orchardServices;

        public UserApprovePartDriver(
            IApproveUserService approveUserService,
            IHttpContextAccessor httpContextAccessor,
            IOrchardServices orchardServices
            ) {

            _approveUserService = approveUserService;
            _httpContextAccessor = httpContextAccessor;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(UserPart part, dynamic shapeHelper) {
            var model = new UserEditViewModel { User = part };

            return ContentShape("Parts_UserApprove_Edit",
                                () => {
                                    if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users"))) {
                                        return null;
                                    }
                                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                                 });
        }

        protected override DriverResult Editor(UserPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new UserEditViewModel { User = part };

            return ContentShape("Parts_UserApprove_Edit",
                              () => {
                                  if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users"))) { 
                                      return null;
                                  }
                                  return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                              });
        }
    }
}