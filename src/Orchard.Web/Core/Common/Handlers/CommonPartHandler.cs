using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.MetaData;
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
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CommonPartHandler(
            IRepository<CommonPartRecord> commonRepository,
            IRepository<CommonPartVersionRecord> commonVersionRepository,
            IClock clock,
            IAuthenticationService authenticationService,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(commonRepository));
            Filters.Add(StorageFilter.For(commonVersionRepository));

            Filters.Add(new ActivatingFilter<ContentPart<CommonPartVersionRecord>>(ContentTypeWithACommonPart));

            OnInitializing<CommonPart>(PropertySetHandlers);
            OnInitializing<CommonPart>(AssignCreatingOwner);
            OnInitializing<CommonPart>(AssignCreatingDates);
            OnInitializing<ContentPart<CommonPartVersionRecord>>(AssignCreatingDates);

            OnLoaded<CommonPart>(LazyLoadHandlers);

            OnVersioned<CommonPart>(AssignVersioningDates);
            OnVersioned<ContentPart<CommonPartVersionRecord>>(AssignVersioningDates);

            OnPublishing<CommonPart>(AssignPublishingDates);
            OnPublishing<ContentPart<CommonPartVersionRecord>>(AssignPublishingDates);

            OnIndexing<CommonPart>((context, commonPart) => context.DocumentIndex
                                                    .Add("type", commonPart.ContentItem.ContentType).Store()
                                                    .Add("author", commonPart.Owner.UserName).Store()
                                                    .Add("created", commonPart.CreatedUtc ?? _clock.UtcNow).Store()
                                                    .Add("published", commonPart.PublishedUtc ?? _clock.UtcNow).Store()
                                                    .Add("modified", commonPart.ModifiedUtc ?? _clock.UtcNow).Store()
                                                    );
        }

        public Localizer T { get; set; }

        bool ContentTypeWithACommonPart(string typeName) {
            return _contentDefinitionManager.GetTypeDefinition(typeName).Parts.Any(part => part.PartDefinition.Name == "CommonPart");
        }

        void AssignCreatingOwner(InitializingContentContext context, CommonPart part) {
            // and use the current user as Owner
            if (part.Record.OwnerId == 0) {
                part.Owner = _authenticationService.GetAuthenticatedUser();
            }
        }

        void AssignCreatingDates(InitializingContentContext context, CommonPart part) {
            // assign default create/modified dates
            part.CreatedUtc = _clock.UtcNow;
            part.ModifiedUtc = _clock.UtcNow;
        }

        void AssignCreatingDates(InitializingContentContext context, ContentPart<CommonPartVersionRecord> part) {
            // assign default create/modified dates
            part.Record.CreatedUtc = _clock.UtcNow;
            part.Record.ModifiedUtc = _clock.UtcNow;
        }

        void AssignVersioningDates(VersionContentContext context, CommonPart existing, CommonPart building) {
            // assign the created
            building.CreatedUtc = existing.CreatedUtc ?? _clock.UtcNow;
            // persist and published dates
            building.PublishedUtc = existing.PublishedUtc;
            // assign modified date for the new version
            building.ModifiedUtc = _clock.UtcNow;
        }

        void AssignVersioningDates(VersionContentContext context, ContentPart<CommonPartVersionRecord> existing, ContentPart<CommonPartVersionRecord> building) {
            // assign the created date
            building.Record.CreatedUtc = _clock.UtcNow;
            // assign modified date for the new version
            building.Record.ModifiedUtc = _clock.UtcNow;
            // publish date should be null until publish method called
            building.Record.PublishedUtc = null;
        }

        void AssignPublishingDates(PublishContentContext context, CommonPart part) {
            // don't assign dates when unpublishing
            if (context.PublishingItemVersionRecord == null)
                return;
            
            // set the initial published date
            part.PublishedUtc = part.PublishedUtc ?? _clock.UtcNow;
        }

        void AssignPublishingDates(PublishContentContext context, ContentPart<CommonPartVersionRecord> part) {
            // don't assign dates when unpublishing
            if (context.PublishingItemVersionRecord == null)
                return;

            // assign the version's published date
            part.Record.PublishedUtc = part.Record.PublishedUtc ?? _clock.UtcNow;
        }

        void LazyLoadHandlers(LoadContentContext context, CommonPart part) {
            // add handlers that will load content for id's just-in-time
            part.OwnerField.Loader(() => _contentManager.Get<IUser>(part.Record.OwnerId));
            part.ContainerField.Loader(() => part.Record.Container == null ? null : _contentManager.Get(part.Record.Container.Id));
        }

        static void PropertySetHandlers(InitializingContentContext context, CommonPart part) {
            // add handlers that will update records when part properties are set

            part.OwnerField.Setter(user => {
                                       part.Record.OwnerId = user == null
                                           ? 0
                                           : user.ContentItem.Id;
                                       return user;
                                   });

            // Force call to setter if we had already set a value
            if (part.OwnerField.Value != null)
                part.OwnerField.Value = part.OwnerField.Value;

            part.ContainerField.Setter(container => {
                                           part.Record.Container = container == null
                                               ? null
                                               : container.ContentItem.Record;
                                           return container;
                                       });

            // Force call to setter if we had already set a value
            if (part.ContainerField.Value != null)
                part.ContainerField.Value = part.ContainerField.Value;
        }
    }
}