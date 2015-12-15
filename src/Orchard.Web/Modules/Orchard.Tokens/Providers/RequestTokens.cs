using System;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Tokens.Providers {
    public class RequestTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;

        public RequestTokens(IWorkContextAccessor workContextAccessor, IContentManager contentManager) {
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Request", T("Http Request"), T("Current Http Request tokens."))
                .Token("QueryString:*", T("QueryString:<element>"), T("The Query String value for the specified element."))
                .Token("Form:*", T("Form:<element>"), T("The Form value for the specified element."))
                .Token("Route:*", T("Route:<key>"), T("The Route value for the specified key."))
                .Token("Content", T("Content"), T("The request routed Content Item."), "Content")
            ;
        }

        public void Evaluate(EvaluateContext context) {
            if (_workContextAccessor.GetContext().HttpContext == null) {
                return;
            }

            context.For("Request", _workContextAccessor.GetContext().HttpContext.Request)
                .Token(
                    token => token.StartsWith("QueryString:", StringComparison.OrdinalIgnoreCase) ? token.Substring("QueryString:".Length) : null,
                    (token, request) => request.QueryString.Get(token)
                )
                .Token(
                    token => token.StartsWith("Form:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Form:".Length) : null,
                    (token, request) => request.Form.Get(token)
                )
                .Token(
                    token => token.StartsWith("Route:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Route:".Length) : null,
                    (token, request) => GetRouteValue(token, request)
                )
                .Token("Content",
                    (request) => DisplayText(GetRoutedContentItem(request))
                )
                .Chain("Content", "Content",
                    (request) => GetRoutedContentItem(request)
                );
        }

        private static string GetRouteValue(string token, HttpRequestBase request) {
            object result;
            if (!request.RequestContext.RouteData.Values.TryGetValue(token, out result)) {
                return String.Empty;
            }

            return result.ToString();
        }

        private ContentItem GetRoutedContentItem(HttpRequestBase request) {
            String area = GetRouteValue("area", request);
            String action = GetRouteValue("action", request);
            int contentId;

            if (!String.Equals(area, "Containers", StringComparison.OrdinalIgnoreCase) && !String.Equals(area, "Contents", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            if (!String.Equals(GetRouteValue("controller", request), "Item", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            if (!int.TryParse(GetRouteValue("id", request), out contentId)) {
                return null;
            }

            if (String.Equals(action, "Display", StringComparison.OrdinalIgnoreCase)) {
                return _contentManager.Get(contentId, VersionOptions.Published);
            }
            else if (String.Equals(action, "Preview", StringComparison.OrdinalIgnoreCase)) {
                VersionOptions versionOptions = VersionOptions.Latest;
                int version;
                if (int.TryParse(request.QueryString["version"], out version)) {
                    versionOptions = VersionOptions.Number(version);
                }
                return _contentManager.Get(contentId, versionOptions);
            }
            else {
                return null;
            }
        }

        private string DisplayText(IContent content) {
            if (content == null) {
                return String.Empty;
            }

            return _contentManager.GetItemMetadata(content).DisplayText;
        }
    }
}