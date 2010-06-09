using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    public class CommonDriver : ContentPartDriver<CommonAspect> {
        private readonly IContentManager _contentManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;
        private readonly IClock _clock;

        public CommonDriver(
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
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(CommonAspect part) {
            return Combined(OwnerEditor(part, null), ContainerEditor(part, null));
        }

        protected override DriverResult Editor(CommonAspect instance, ContentManagement.IUpdateModel updater) {
            // this event is hooked so the modified timestamp is changed when an edit-post occurs.            
            instance.ModifiedUtc = _clock.UtcNow;
            instance.VersionModifiedUtc = _clock.UtcNow;

            return Combined(OwnerEditor(instance, updater), ContainerEditor(instance, updater));
        }

        DriverResult OwnerEditor(CommonAspect part, IUpdateModel updater) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, part)) {
                return null;
            }

            var model = new OwnerEditorViewModel();
            if (part.Owner != null)
                model.Owner = part.Owner.UserName;

            if (updater != null) {
                var priorOwner = model.Owner;
                updater.TryUpdateModel(model, "CommonAspect", null, null);

                if (model.Owner != null && model.Owner != priorOwner) {
                    var newOwner = _membershipService.GetUser(model.Owner);
                    if (newOwner == null) {
                        updater.AddModelError("CommonAspect.Owner", T("Invalid user name"));
                    }
                    else {
                        part.Owner = newOwner;
                    }
                }
            }

            return ContentPartTemplate(model, "Parts/Common.Owner", "CommonAspect").Location("primary", "10");
        }

        DriverResult ContainerEditor(CommonAspect part, IUpdateModel updater) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, part)) {
                return null;
            }

            var model = new ContainerEditorViewModel();
            if (part.Container != null)
                model.ContainerId = part.Container.ContentItem.Id;

            if (updater != null) {
                var priorContainerId = model.ContainerId;
                updater.TryUpdateModel(model, "CommonAspect", null, null);

                if (model.ContainerId != null && model.ContainerId != priorContainerId) {
                    var newContainer = _contentManager.Get((int)model.ContainerId, VersionOptions.Latest);
                    if (newContainer == null) {
                        updater.AddModelError("CommonAspect.ContainerId", T("Invalid container"));
                    }
                    else {
                        part.Container = newContainer;
                    }
                }
            }
            return ContentPartTemplate(model, "Parts/Common.Container", "CommonAspect").Location("primary", "10.1");
        }
    }
}