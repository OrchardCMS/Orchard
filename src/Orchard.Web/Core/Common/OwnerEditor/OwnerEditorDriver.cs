using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Core.Common.OwnerEditor {
    public class OwnerEditorDriver : ContentPartDriver<CommonPart> {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;

        public OwnerEditorDriver(
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
            get { return "OwnerEditor"; }
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(StandardPermissions.SiteOwner, currentUser, part)) {
                return null;
            }

            
            var settings = part.TypePartDefinition.Settings.GetModel<OwnerEditorSettings>();
            if (!settings.ShowOwnerEditor) {
                if (part.Owner == null) {
                    part.Owner = currentUser;
                }
                return null;
            }

            return ContentShape(
                "Parts_Common_Owner_Edit",
                () => {
                    OwnerEditorViewModel model = shapeHelper.Parts_Common_Owner_Edit(typeof(OwnerEditorViewModel));

                    if (part.Owner != null) {
                        model.Owner = part.Owner.UserName;
                    }

                    if (updater != null) {
                        var priorOwner = model.Owner;
                        updater.TryUpdateModel(model, Prefix, null, null);

                        if (model.Owner != null && model.Owner != priorOwner) {
                            var newOwner = _membershipService.GetUser(model.Owner);
                            if (newOwner == null) {
                                updater.AddModelError("OwnerEditor.Owner", T("Invalid user name"));
                            }
                            else {
                                part.Owner = newOwner;
                            }
                        }
                    }
                    return model;
                });
        }
    }
}