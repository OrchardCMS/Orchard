using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Aspects;
using Orchard.Models.Driver;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Providers {
    public class CommonAspectProvider : ContentProvider {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;

        public CommonAspectProvider(
            IRepository<CommonRecord> repository,
            IClock clock,
            IAuthenticationService authenticationService,
            IContentManager contentManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _contentManager = contentManager;

            OnActivated<CommonAspect>(PropertySetHandlers);
            OnCreating<CommonAspect>(DefaultTimestampsAndOwner);
            OnLoaded<CommonAspect>(LazyLoadHandlers);
            Filters.Add(new StorageFilter<CommonRecord>(repository));
        }

        void DefaultTimestampsAndOwner(CreateContentContext context, CommonAspect instance) {
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


        protected override void UpdateEditorViewModel(UpdateEditorViewModelContext context) {
            var part = context.ContentItem.As<CommonAspect>();
            if (part == null)
                return;

            // this event is hooked so the modified timestamp is changed when an edit-post occurs.
            // kind of a loose rule of thumb. may not be sufficient
            part.Record.ModifiedUtc = _clock.UtcNow;
        }

    }
}