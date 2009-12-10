using System;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Models.Aspects;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Providers {
    public class CommonAspectHandler : ContentHandler {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;

        public CommonAspectHandler(
            IRepository<CommonRecord> repository,
            IClock clock,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IMembershipService membershipService,
            IContentManager contentManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _membershipService = membershipService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;

            Filters.Add(new StorageFilter<CommonRecord>(repository));

            OnActivated<CommonAspect>(PropertySetHandlers);
            OnActivated<CommonAspect>(DefaultTimestampsAndOwner);
            OnLoaded<CommonAspect>(LazyLoadHandlers);

            //OnGetDisplayViewModel<CommonAspect>();
            OnGetEditorViewModel<CommonAspect>(GetEditor);
            OnUpdateEditorViewModel<CommonAspect>(UpdateEditor);
        }

        public Localizer T { get; set; }

        void DefaultTimestampsAndOwner(ActivatedContentContext context, CommonAspect instance) {
            // assign default create/modified dates
            if (instance.Record.CreatedUtc == null) {
                instance.Record.CreatedUtc = _clock.UtcNow;
            }
            if (instance.Record.ModifiedUtc == null) {
                instance.Record.ModifiedUtc = _clock.UtcNow;
            }

            // and use the current user as Owner
            if (instance.Record.OwnerId == 0) {
                ((ICommonAspect)instance).Owner = _authenticationService.GetAuthenticatedUser();
            }
        }

        void LazyLoadHandlers(LoadContentContext context, CommonAspect aspect) {
            // add handlers that will load content for id's just-in-time
            aspect.OwnerField.Loader(() => _contentManager.Get<IUser>(aspect.Record.OwnerId));
            aspect.ContainerField.Loader(() => aspect.Record.Container == null ? null : _contentManager.Get(aspect.Record.Container.Id));
        }

        static void PropertySetHandlers(ActivatedContentContext context, CommonAspect aspect) {
            // add handlers that will update records when aspect properties are set

            aspect.OwnerField.Setter(user => {
                if (user == null) {
                    aspect.Record.OwnerId = 0;
                }
                else {
                    aspect.Record.OwnerId = user.ContentItem.Id;
                }
                return user;
            });

            aspect.ContainerField.Setter(container => {
                if (container == null) {
                    aspect.Record.Container = null;
                }
                else {
                    aspect.Record.Container = container.ContentItem.Record;
                }
                return container;
            });
        }


        private void GetEditor(BuildEditorModelContext context, CommonAspect instance) {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.CheckAccess(currentUser, Permissions.ChangeOwner)) {
                return;
            }
            var viewModel = new OwnerEditorViewModel();
            if (instance.Owner != null)
                viewModel.Owner = instance.Owner.UserName;

            context.AddEditor(new TemplateViewModel(viewModel, "CommonAspect"));
        }


        private void UpdateEditor(UpdateEditorModelContext context, CommonAspect instance) {
            // this event is hooked so the modified timestamp is changed when an edit-post occurs.
            // kind of a loose rule of thumb. may not be sufficient
            instance.Record.ModifiedUtc = _clock.UtcNow;

            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.CheckAccess(currentUser, Permissions.ChangeOwner)) {
                return;
            }

            var viewModel = new OwnerEditorViewModel ();
            if (instance.Owner != null)
                viewModel.Owner = instance.Owner.UserName;

            var priorOwner = viewModel.Owner;
            context.Updater.TryUpdateModel(viewModel, "CommonAspect", null, null);

            if (viewModel.Owner != priorOwner) {
                var newOwner = _membershipService.GetUser(viewModel.Owner);
                if (newOwner == null) {
                    context.Updater.AddModelError("CommonAspect.Owner", T("Invalid user name"));
                }
                else {
                    instance.Owner = newOwner;
                }
            }
            context.AddEditor(new TemplateViewModel(viewModel, "CommonAspect"));
        }
    }
}