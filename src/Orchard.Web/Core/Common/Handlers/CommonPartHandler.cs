using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class CommonPartHandler : ContentHandler {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;

        public CommonPartHandler(
            IRepository<CommonPartRecord> commonRepository,
            IRepository<CommonPartVersionRecord> commonVersionRepository,
            IClock clock,
            IAuthenticationService authenticationService,
            IContentManager contentManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(commonRepository));
            Filters.Add(StorageFilter.For(commonVersionRepository));

            OnInitializing<CommonPart>(PropertySetHandlers);
            OnInitializing<CommonPart>(AssignCreatingOwner);
            OnInitializing<ContentPart<CommonPartRecord>>(AssignCreatingDates);
            OnInitializing<ContentPart<CommonPartVersionRecord>>(AssignCreatingDates);

            OnLoaded<CommonPart>(LazyLoadHandlers);

            OnVersioning<CommonPart>(CopyOwnerAndContainer);

            OnVersioned<ContentPart<CommonPartVersionRecord>>(AssignVersioningDates);

            OnPublishing<ContentPart<CommonPartRecord>>(AssignPublishingDates);
            OnPublishing<ContentPart<CommonPartVersionRecord>>(AssignPublishingDates);

            //OnGetDisplayViewModel<CommonPart>();
            //OnGetEditorViewModel<CommonPart>(GetEditor);
            //OnUpdateEditorViewModel<CommonPart>(UpdateEditor);

            OnIndexing<CommonPart>((context, commonPart) => context.DocumentIndex
                                                    .Add("type", commonPart.ContentItem.ContentType).Store()
                                                    .Add("author", commonPart.Owner.UserName).Store()
                                                    .Add("created", commonPart.CreatedUtc ?? _clock.UtcNow).Store()
                                                    .Add("published", commonPart.PublishedUtc ?? _clock.UtcNow).Store()
                                                    .Add("modified", commonPart.ModifiedUtc ?? _clock.UtcNow).Store()
                                                    );
        }

        public Localizer T { get; set; }


        void AssignCreatingOwner(InitializingContentContext context, CommonPart part) {
            // and use the current user as Owner
            if (part.Record.OwnerId == 0) {
                part.Owner = _authenticationService.GetAuthenticatedUser();
            }
        }

        void AssignCreatingDates(InitializingContentContext context, ContentPart<CommonPartRecord> part) {
            // assign default create/modified dates
            part.Record.CreatedUtc = _clock.UtcNow;
            part.Record.ModifiedUtc = _clock.UtcNow;
        }

        void AssignCreatingDates(InitializingContentContext context, ContentPart<CommonPartVersionRecord> part) {
            // assign default create/modified dates
            part.Record.CreatedUtc = _clock.UtcNow;
            part.Record.ModifiedUtc = _clock.UtcNow;
        }

        void AssignVersioningDates(VersionContentContext context, ContentPart<CommonPartVersionRecord> existing, ContentPart<CommonPartVersionRecord> building) {
            // assign create/modified dates for the new version
            building.Record.CreatedUtc = _clock.UtcNow;
            building.Record.ModifiedUtc = _clock.UtcNow;

            // publish date should be null until publish method called
            building.Record.PublishedUtc = null;
        }

        void AssignPublishingDates(PublishContentContext context, ContentPart<CommonPartRecord> part) {
            // don't assign dates when unpublishing
            if (context.PublishingItemVersionRecord == null)
                return;

            // assign version-agnostic publish date
            part.Record.PublishedUtc = _clock.UtcNow;
        }

        void AssignPublishingDates(PublishContentContext context, ContentPart<CommonPartVersionRecord> part) {
            // don't assign dates when unpublishing
            if (context.PublishingItemVersionRecord == null)
                return;

            // assign version-specific publish date
            part.Record.PublishedUtc = _clock.UtcNow;
        }

        private static void CopyOwnerAndContainer(VersionContentContext c, CommonPart c1, CommonPart c2) {
            c2.Owner = c1.Owner;
            c2.Container = c1.Container;
        }

        void LazyLoadHandlers(LoadContentContext context, CommonPart part) {
            // add handlers that will load content for id's just-in-time
            part.OwnerField.Loader(() => _contentManager.Get<IUser>(part.Record.OwnerId));
            part.ContainerField.Loader(() => part.Record.Container == null ? null : _contentManager.Get(part.Record.Container.Id));
        }

        static void PropertySetHandlers(InitializingContentContext context, CommonPart part) {
            // add handlers that will update records when part properties are set

            part.OwnerField.Setter(user => {
                                         if (user == null) {
                                             part.Record.OwnerId = 0;
                                         }
                                         else {
                                             part.Record.OwnerId = user.ContentItem.Id;
                                         }
                                         return user;
                                     });

            // Force call to setter if we had already set a value
            if (part.OwnerField.Value != null)
                part.OwnerField.Value = part.OwnerField.Value;

            part.ContainerField.Setter(container => {
                                             if (container == null) {
                                                 part.Record.Container = null;
                                             }
                                             else {
                                                 part.Record.Container = container.ContentItem.Record;
                                             }
                                             return container;
                                         });

            // Force call to setter if we had already set a value
            if (part.ContainerField.Value != null)
                part.ContainerField.Value = part.ContainerField.Value;
        }


        //private void GetEditor(BuildEditorContext context, CommonPart instance) {
        //    var currentUser = _authenticationService.GetAuthenticatedUser();
        //    if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, instance)) {
        //        return;
        //    }
        //    var viewModel = new OwnerEditorViewModel();
        //    if (instance.Owner != null)
        //        viewModel.Owner = instance.Owner.UserName;

        //    context.AddEditor(new TemplateViewModel(viewModel, "CommonPart") { TemplateName = "Parts/Common.Owner", ZoneName = "primary", Position = "999" });
        //}


        //private void UpdateEditor(UpdateEditorContext context, CommonPart instance) {
        //    // this event is hooked so the modified timestamp is changed when an edit-post occurs.
        //    // kind of a loose rule of thumb. may not be sufficient
        //    instance.ModifiedUtc = _clock.UtcNow;
        //    instance.VersionModifiedUtc = _clock.UtcNow;

        //    var currentUser = _authenticationService.GetAuthenticatedUser();
        //    if (!_authorizationService.TryCheckAccess(Permissions.ChangeOwner, currentUser, instance)) {
        //        return;
        //    }

        //    var viewModel = new OwnerEditorViewModel();
        //    if (instance.Owner != null)
        //        viewModel.Owner = instance.Owner.UserName;

        //    var priorOwner = viewModel.Owner;
        //    context.Updater.TryUpdateModel(viewModel, "CommonPart", null, null);

        //    if (viewModel.Owner != null && viewModel.Owner != priorOwner) {
        //        var newOwner = _membershipService.GetUser(viewModel.Owner);
        //        if (newOwner == null) {
        //            context.Updater.AddModelError("CommonPart.Owner", T("Invalid user name"));
        //        }
        //        else {
        //            instance.Owner = newOwner;
        //        }
        //    }

        //    context.AddEditor(new TemplateViewModel(viewModel, "CommonPart") { TemplateName = "Parts/Common.Owner", ZoneName = "primary", Position = "999" });
        //}
    }
}