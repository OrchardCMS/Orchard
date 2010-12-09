using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Security;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Common.Scheduling {
    public abstract class OwnedScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IOrchardServices _orchardServices;

        protected OwnedScheduledTaskHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected void SetCurrentUser(ContentItem contentItem) {
            IUser owner = null;
            var commonPart = contentItem.As<CommonPart>();
            if (commonPart != null) {
                owner = commonPart.Owner;
            }
            if (owner == null) {
                var superUser = _orchardServices.WorkContext.CurrentSite.SuperUser;
                owner = _orchardServices.WorkContext.Resolve<IMembershipService>().GetUser(superUser);
            }
            _orchardServices.WorkContext.Resolve<IAuthenticationService>().SetAuthenticatedUserForRequest(owner);
        }

        public abstract void Process(ScheduledTaskContext context);
    }
}