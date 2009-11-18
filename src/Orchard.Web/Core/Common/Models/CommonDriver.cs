using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.Common.Models {
    public class CommonDriver : ModelDriverWithRecord<CommonRecord> {
        private readonly IClock _clock;
        private readonly IAuthenticationService _authenticationService;
        private readonly IModelManager _modelManager;

        public CommonDriver(
            IRepository<CommonRecord> repository,
            IClock clock,
            IAuthenticationService authenticationService,
            IModelManager modelManager)
            : base(repository) {
            _clock = clock;
            _authenticationService = authenticationService;
            _modelManager = modelManager;
        }

        protected override void Create(CreateModelContext context) {
            var instance = context.Instance.As<CommonModel>();
            if (instance != null && instance.Record != null) {
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

            base.Create(context);
        }

        protected override void Loaded(LoadModelContext context) {
            var instance = context.Instance.As<CommonModel>();
            if (instance != null && instance.Record != null) {
                if (instance.Record.OwnerId != 0) {
                    instance.Owner = _modelManager.Get(instance.Record.OwnerId).As<IUser>();
                }
            }
            base.Loaded(context);
        }
    }
}