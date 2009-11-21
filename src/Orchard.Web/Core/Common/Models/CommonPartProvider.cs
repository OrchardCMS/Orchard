using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Models {
    public class CommonPartProvider : ContentProvider {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;

        public CommonPartProvider(
            IRepository<CommonRecord> repository,
            IClock clock,
            IAuthenticationService authenticationService,
            IContentManager contentManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _contentManager = contentManager;

            AddOnCreating<CommonPart>(SetCreateTimesAndAuthor);
            Filters.Add(new StorageFilter<CommonRecord>(repository));
            AddOnLoaded<CommonPart>(LoadOwnerModel);
        }

        void SetCreateTimesAndAuthor(CreateContentContext context, CommonPart instance) {
            if (instance.Record.CreatedUtc == null) {
                instance.Record.CreatedUtc = _clock.UtcNow;
            }
            if (instance.Record.ModifiedUtc == null) {
                instance.Record.ModifiedUtc = _clock.UtcNow;
            }
            if (instance.Record.OwnerId == 0) {
                instance.Owner = _authenticationService.GetAuthenticatedUser();
                if (instance.Owner != null)
                    instance.Record.OwnerId = instance.Owner.Id;
            }
        }

        protected override void UpdateEditors(UpdateContentContext context) {
            var part = context.ContentItem.As<CommonPart>();
            if (part==null)
                return;

            part.Record.ModifiedUtc = _clock.UtcNow;
        }

        void LoadOwnerModel(LoadContentContext context, CommonPart part) {
            part.LoadOwner(() => _contentManager.Get<IUser>(part.Record.OwnerId));
            part.LoadContainer(() => part.Record.Container == null ? null : _contentManager.Get(part.Record.Container.Id));
        }
    }
}