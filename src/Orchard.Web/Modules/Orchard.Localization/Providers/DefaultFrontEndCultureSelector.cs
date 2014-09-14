using System;
using System.Web;
using System.Web.Routing;
using Orchard.Alias;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.UI.Admin;

namespace Orchard.Localization.Providers {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class DefaultFrontEndCultureSelector : ICultureSelector {
        private readonly IAliasService _aliasService;
        private readonly IContentManager _contentManager;
        private readonly Lazy<ILocalizationService> _localizationService;

        public DefaultFrontEndCultureSelector(IAliasService aliasService,
            IContentManager contentManager,
            Lazy<ILocalizationService> localizationService) {
            _aliasService = aliasService;
            _contentManager = contentManager;
            _localizationService = localizationService;
        }

        private bool IsActivable(HttpContextBase context) {
            // activate on front end screen only
            if (AdminFilter.IsApplied(new RequestContext(context, new RouteData())))
                return false;

            return true;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (!IsActivable(context))
                return null;

            var content = GetByPath(context.Request.Path.TrimStart('/'));
            if (content != null) {
                return new CultureSelectorResult {Priority = -1, CultureName = _localizationService.Value.GetContentCulture(content)};
            }

            return null;
        }

        public IContent GetByPath(string aliasPath) {
            var contentRouting = _aliasService.Get(aliasPath);

            if (contentRouting == null)
                return null;

            object id;
            if (contentRouting.TryGetValue("id", out id)) {
                int contentId;
                if (int.TryParse(id as string, out contentId)) {
                    return _contentManager.Get(contentId);
                }
            }

            return null;
        }
    }
}