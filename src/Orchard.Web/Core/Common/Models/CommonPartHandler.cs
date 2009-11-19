using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Models {
    public class CommonPartHandler : ContentHandler {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;

        public CommonPartHandler(
            IRepository<CommonRecord> repository,
            IClock clock,
            IAuthenticationService authenticationService,
            IContentManager contentManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _contentManager = contentManager;

            AddOnCreating<CommonPart>(SetCreateTimesAndAuthor);
            Filters.Add(new StorageFilterForRecord<CommonRecord>(repository));
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

        void LoadOwnerModel(LoadContentContext context, CommonPart instance) {
            if (instance.Record.OwnerId != 0) {
                instance.Owner = _contentManager.Get(instance.Record.OwnerId).As<IUser>();
            }
        }
    }
}