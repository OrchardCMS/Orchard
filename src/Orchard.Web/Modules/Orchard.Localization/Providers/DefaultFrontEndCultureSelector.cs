using System;
using System.Web;
using System.Web.Mvc;
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

            // Attempt to determine culture by route.
            // This normally happens when you are using non standard pages that are not content items
            // {culture}/foo
            var routeCulture = context.Request.RequestContext.RouteData.Values["culture"] ??
                context.Request.RequestContext.HttpContext.Request.Params["culture"];
            if (routeCulture != null && !string.IsNullOrWhiteSpace(routeCulture.ToString())) {
                return new CultureSelectorResult { Priority = -1, CultureName = routeCulture.ToString() };
            }

            // Attempt to determine culture by previous route if by POST
            string path = string.Empty;
            if (context.Request.HttpMethod.Equals(HttpVerbs.Post.ToString(), StringComparison.OrdinalIgnoreCase)) {
                path = context.Request.UrlReferrer.AbsolutePath;
            }
            else {
                path = context.Request.Path;
            }

            var content = GetByPath(path.TrimStart('/'));
            if (content != null) {
                return new CultureSelectorResult { Priority = -1, CultureName = _localizationService.Value.GetContentCulture(content) };
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