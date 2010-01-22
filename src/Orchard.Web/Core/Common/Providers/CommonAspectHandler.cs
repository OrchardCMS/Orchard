using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Providers {
    [UsedImplicitly]
    public class CommonAspectHandler : ContentHandler {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;

        public CommonAspectHandler(
            IRepository<CommonRecord> commonRepository,
            IRepository<CommonVersionRecord> commonVersionRepository,
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

            Filters.Add(StorageFilter.For(commonRepository));
            Filters.Add(StorageFilter.For(commonVersionRepository));

            OnActivated<CommonAspect>(PropertySetHandlers);
            OnActivated<CommonAspect>(AssignCreatingOwner);
            OnActivated <ContentPart<CommonRecord>>(AssignCreatingDates);
            OnActivated<ContentPart<CommonVersionRecord>>(AssignCreatingDates);
            OnVersioned<ContentPart<CommonVersionRecord>>(AssignVersioningDates);
            OnPublishing<ContentPart<CommonRecord>>(AssignPublishingDates);
            OnPublishing<ContentPart<CommonVersionRecord>>(AssignPublishingDates);
            OnLoaded<CommonAspect>(LazyLoadHandlers);

            //OnGetDisplayViewModel<CommonAspect>();
            OnGetEditorViewModel<CommonAspect>(GetEditor);
            OnUpdateEditorViewModel<CommonAspect>(UpdateEditor);
        }

        public Localizer T { get; set; }


        void AssignCreatingOwner(ActivatedContentContext context, CommonAspect part) {     
            // and use the current user as Owner
            if (part.Record.OwnerId == 0) {
                part.Owner = _authenticationService.GetAuthenticatedUser();
            }
        }

        void AssignCreatingDates(ActivatedContentContext context, ContentPart<CommonRecord> part) {
            // assign default create/modified dates
            part.Record.CreatedUtc = _clock.UtcNow;
            part.Record.ModifiedUtc = _clock.UtcNow;
        }

        void AssignCreatingDates(ActivatedContentContext context, ContentPart<CommonVersionRecord> part) {
            // assign default create/modified dates
            part.Record.CreatedUtc = _clock.UtcNow;
            part.Record.ModifiedUtc = _clock.UtcNow;
        }

        void AssignVersioningDates(VersionContentContext context, ContentPart<CommonVersionRecord> existing, ContentPart<CommonVersionRecord> building) {
            // assign create/modified dates for the new version
            building.Record.CreatedUtc = _clock.UtcNow;
            building.Record.ModifiedUtc = _clock.UtcNow;

            // publish date should be null until publish method called
            building.Record.PublishedUtc = null;
        }

        void AssignPublishingDates(PublishContentContext context, ContentPart<CommonRecord> part) {
            // don't assign dates when unpublishing
            if (context.PublishingItemVersionRecord == null)
                return;

            // assign version-agnostic publish date
            part.Record.PublishedUtc = _clock.UtcNow;
        }

        void AssignPublishingDates(PublishContentContext context, ContentPart<CommonVersionRecord> part) {
            // don't assign dates when unpublishing
            if (context.PublishingItemVersionRecord == null)
                return;

            // assign version-specific publish date
            part.Record.PublishedUtc = _clock.UtcNow;
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
            if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, instance)) {
                return;
            }
            var viewModel = new OwnerEditorViewModel();
            if (instance.Owner != null)
                viewModel.Owner = instance.Owner.UserName;

            context.AddEditor(new TemplateViewModel(viewModel, "CommonAspect") { TemplateName = "Parts/Common.Owner", ZoneName = "primary", Position = "999" });
        }


        private void UpdateEditor(UpdateEditorModelContext context, CommonAspect instance) {
            // this event is hooked so the modified timestamp is changed when an edit-post occurs.
            // kind of a loose rule of thumb. may not be sufficient
            instance.ModifiedUtc = _clock.UtcNow;
            instance.VersionModifiedUtc = _clock.UtcNow;

            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, instance)) {
                return;
            }

            var viewModel = new OwnerEditorViewModel();
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
            context.AddEditor(new TemplateViewModel(viewModel, "CommonAspect") { TemplateName = "Parts/Common.Owner", ZoneName = "primary", Position = "999" });
        }
    }
}