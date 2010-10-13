using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    public class CommonPartDriver : ContentPartDriver<CommonPart> {
        private const string TemplatePrefix = "CommonPart";
        private readonly IContentManager _contentManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;
        private readonly IClock _clock;

        public CommonPartDriver(
            IOrchardServices services,
            IContentManager contentManager,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IMembershipService membershipService,
            IClock clock) {
            _contentManager = contentManager;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _membershipService = membershipService;
            _clock = clock;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(CommonPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Common_Metadata",
                             () => shapeHelper.Parts_Common_Metadata(ContentPart: part)),
                ContentShape("Parts_Common_Metadata_Summary",
                             () => shapeHelper.Parts_Common_Metadata_Summary(ContentPart: part)),
                ContentShape("Parts_Common_Metadata_SummaryAdmin",
                             () => shapeHelper.Parts_Common_Metadata_SummaryAdmin(ContentPart: part))
                );
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return Combined(
                OwnerEditor(part, null),
                ContainerEditor(part, null));
        }

        protected override DriverResult Editor(CommonPart instance, IUpdateModel updater, dynamic shapeHelper) {
            // this event is hooked so the modified timestamp is changed when an edit-post occurs.            
            instance.ModifiedUtc = _clock.UtcNow;
            instance.VersionModifiedUtc = _clock.UtcNow;

            return Combined(
                OwnerEditor(instance, updater),
                ContainerEditor(instance, updater));
        }

        DriverResult OwnerEditor(CommonPart part, IUpdateModel updater) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, part)) {
                return null;
            }

            var model = new OwnerEditorViewModel();
            if (part.Owner != null)
                model.Owner = part.Owner.UserName;

            if (updater != null) {
                var priorOwner = model.Owner;
                updater.TryUpdateModel(model, TemplatePrefix, null, null);

                if (model.Owner != null && model.Owner != priorOwner) {
                    var newOwner = _membershipService.GetUser(model.Owner);
                    if (newOwner == null) {
                        updater.AddModelError("CommonPart.Owner", T("Invalid user name"));
                    }
                    else {
                        part.Owner = newOwner;
                    }
                }
            }

            return ContentPartTemplate(model, "Parts/Common.Owner", TemplatePrefix).Location(part.GetLocation("Editor"));
        }

        DriverResult ContainerEditor(CommonPart part, IUpdateModel updater) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, part)) {
                return null;
            }

            var model = new ContainerEditorViewModel();
            if (part.Container != null)
                model.ContainerId = part.Container.ContentItem.Id;

            if (updater != null) {
                var priorContainerId = model.ContainerId;
                updater.TryUpdateModel(model, TemplatePrefix, null, null);

                if (model.ContainerId != null && model.ContainerId != priorContainerId) {
                    var newContainer = _contentManager.Get((int)model.ContainerId, VersionOptions.Latest);
                    if (newContainer == null) {
                        updater.AddModelError("CommonPart.ContainerId", T("Invalid container"));
                    }
                    else {
                        part.Container = newContainer;
                    }
                }
            }

            return ContentPartTemplate(model, "Parts/Common.Container", TemplatePrefix).Location(part.GetLocation("Editor"));
        }
    }
}