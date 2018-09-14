using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Glimpse.Services;
using Orchard.Glimpse.Tabs.Authorizer;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Glimpse.AlternateImplementation {
    [OrchardFeature(FeatureNames.Authorizer)]
    public class GlimpseAuthorizerDecorator : IDecorator<IAuthorizer>, IAuthorizer {
        private readonly IAuthorizer _decoratedService;
        private readonly IGlimpseService _glimpseService;

        public GlimpseAuthorizerDecorator(IAuthorizer decoratedService, IGlimpseService glimpseService) {
            _decoratedService = decoratedService;
            _glimpseService = glimpseService;
        }

        public bool Authorize(Permission permission) {
            return _glimpseService.PublishTimedAction(() => _decoratedService.Authorize(permission),
                (r, t) => new AuthorizerMessage {
                    Permission = permission,
                    Result = r,
                    Duration = t.Duration
                }, TimelineCategories.Authorizer, "Authorize", permission.Name).ActionResult;
        }

        public bool Authorize(Permission permission, LocalizedString message) {
            return _glimpseService.PublishTimedAction(() => _decoratedService.Authorize(permission, message),
                (r, t) => new AuthorizerMessage {
                    Permission = permission,
                    Message = message.Text,
                    Result = r,
                    Duration = t.Duration
                }, TimelineCategories.Authorizer, "Authorize", permission.Name).ActionResult;
        }

        public bool Authorize(Permission permission, IContent content) {
            return _glimpseService.PublishTimedAction(() => _decoratedService.Authorize(permission, content),
                (r, t) => new AuthorizerMessage {
                    Permission = permission,
                    Content = content,
                    Result = r,
                    Duration = t.Duration
                }, TimelineCategories.Authorizer, "Authorize", permission.Name).ActionResult;
        }

        public bool Authorize(Permission permission, IContent content, LocalizedString message) {
            return _glimpseService.PublishTimedAction(() => _decoratedService.Authorize(permission, content, message),
                (r, t) => new AuthorizerMessage {
                    Permission = permission,
                    Content = content,
                    Message = message.Text,
                    Result = r,
                    Duration = t.Duration
                }, TimelineCategories.Authorizer, "Authorize", permission.Name).ActionResult;
        }
    }
}