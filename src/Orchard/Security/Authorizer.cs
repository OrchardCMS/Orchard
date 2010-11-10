using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;

namespace Orchard.Security {
    public interface IAuthorizer : IDependency {
        bool Authorize(Permission permission);
        bool Authorize(Permission permission, LocalizedString message);
        bool Authorize(Permission permission, IContent content, LocalizedString message);
    }

    public class Authorizer : IAuthorizer {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IWorkContextAccessor _workContextAccessor;

        public Authorizer(
            IAuthorizationService authorizationService,
            INotifier notifier,
            IWorkContextAccessor workContextAccessor) {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool Authorize(Permission permission) {
            return Authorize(permission, null, null);
        }

        public bool Authorize(Permission permission, LocalizedString message) {
            return Authorize(permission, null, message);
        }

        public bool Authorize(Permission permission, IContent content, LocalizedString message) {
            if (_authorizationService.TryCheckAccess(permission, _workContextAccessor.GetContext().CurrentUser, content))
                return true;

            if (message != null) {
                if (_workContextAccessor.GetContext().CurrentUser == null) {
                    _notifier.Error(T("{0}. Anonymous users do not have {1} permission.",
                                      message, permission.Name));
                }
                else {
                    _notifier.Error(T("{0}. Current user, {2}, does not have {1} permission.",
                                      message, permission.Name, _workContextAccessor.GetContext().CurrentUser.UserName));
                }
            }

            return false;
        }

    }
}
