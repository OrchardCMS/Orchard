using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Models {
    public class CommonDriver : ModelDriver {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;

        public CommonDriver(
            IRepository<CommonRecord> repository,
            IClock clock,
            IAuthenticationService authenticationService,
            IContentManager contentManager) {

            _clock = clock;
            _authenticationService = authenticationService;
            _contentManager = contentManager;

            AddOnCreating<CommonModel>(SetCreateTimesAndAuthor);
            Filters.Add(new StorageFilterForRecord<CommonRecord>(repository));
            AddOnLoaded<CommonModel>(LoadOwnerModel);
        }

        void SetCreateTimesAndAuthor(CreateModelContext context, CommonModel instance) {
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

        void LoadOwnerModel(LoadModelContext context, CommonModel instance) {
            if (instance.Record.OwnerId != 0) {
                instance.Owner = _contentManager.Get(instance.Record.OwnerId).As<IUser>();
            }
        }
    }
}