using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Core.Common.Drivers {
    public class OwnerEditorPartDriver : ContentPartDriver<CommonPart> {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;

        public OwnerEditorPartDriver(
            IOrchardServices services,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IMembershipService membershipService) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "OwnerEditorPart"; }
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(StandardPermissions.SiteOwner, currentUser, part)) {
                return null;
            }

            var commonEditorsSettings = CommonEditorsSettings.Get(part.ContentItem);
            if (!commonEditorsSettings.ShowOwnerEditor) {
                return null;
            }

            var model = new OwnerEditorViewModel();
            if (part.Owner != null)
                model.Owner = part.Owner.UserName;

            if (updater != null) {
                var priorOwner = model.Owner;
                updater.TryUpdateModel(model, Prefix, null, null);

                if (model.Owner != null && model.Owner != priorOwner) {
                    var newOwner = _membershipService.GetUser(model.Owner);
                    if (newOwner == null) {
                        updater.AddModelError("CommonPart.Owner", T("Invalid user name"));
                    } else {
                        part.Owner = newOwner;
                    }
                }
            }

            return ContentShape("Parts_Common_Owner_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Common.Owner", Model: model, Prefix: Prefix));
        }
    }
}