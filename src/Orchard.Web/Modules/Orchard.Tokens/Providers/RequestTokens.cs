using System;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Localization;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Orchard.Tokens.Providers {
    public class RequestTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private static string[] _textChainableTokens;

        public RequestTokens(IWorkContextAccessor workContextAccessor, IContentManager contentManager) {
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _textChainableTokens = new string[] { "QueryString", "Form" };
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Request", T("Http Request"), T("Current Http Request tokens."))
                .Token("QueryString:*", T("QueryString:<element>"), T("The Query String value for the specified element. To chain text, surround the <element> with parentheses, e.g. 'QueryString:(param1)'."))
                .Token("Form:*", T("Form:<element>"), T("The Form value for the specified element. To chain text, surround the <element> with parentheses, e.g. 'Form:(param1)'."))
                .Token("Route:*", T("Route:<key>"), T("The Route value for the specified key."))
                .Token("Content", T("Content"), T("The request routed Content Item."), "Content")
            ;
        }

        public void Evaluate(EvaluateContext context) {
            if (_workContextAccessor.GetContext().HttpContext == null) {
                return;
            }
            /* Supported Syntaxes for Request and Form tokens are:
             * 1. QueryString:(param1) or Form:(param1) 
             * 2. QueryString:param1 or Form:param1
             * 3. QueryString:(param1).SomeOtherTextToken or Form:(param1).SomeOtherTextToken
             * 
             * If you want to Chain TextTokens you have to use the 3rd syntax
             * the element, here param1, has been surrounded with parentheses in order to preserve backward compatibility.
             */
            context.For("Request", _workContextAccessor.GetContext().HttpContext.Request)
                .Token(
                    FilterTokenParam,
                    (token, request) => {
                        return request.QueryString.Get(token);
                    }
                )
                .Token(
                    FilterTokenParam,
                    (token, request) => request.Form.Get(token)
                )
                .Token(
                    token => token.StartsWith("Route:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Route:".Length) : null,
                    (token, request) => GetRouteValue(token, request)
                )
                .Chain(FilterChainParam, "Text", (token, request) => request.QueryString.Get(token))
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

        private static string FilterTokenParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;
            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!_textChainableTokens.Contains(tokenPrefix, StringComparer.OrdinalIgnoreCase)) {
                return null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = (tokenPrefix + ":").Length;
            if (chainIndex == 0) {// ")." has not be found
                return token.Substring(tokenLength).Trim(new char[] { '(', ')' });
            }
            if (!token.StartsWith((tokenPrefix + ":"), StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength) {
                return null;
            }
            return token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' });
        }
        private static Tuple<string, string> FilterChainParam(string token) {
            string tokenPrefix;
            int chainIndex, tokenLength;

            if (token.IndexOf(":") == -1) {
                return null;
            }
            tokenPrefix = token.Substring(0, token.IndexOf(":"));
            if (!_textChainableTokens.Contains(tokenPrefix, StringComparer.OrdinalIgnoreCase)) {
                return null;
            }

            // use ")." as chars combination to discover the end of the parameter
            chainIndex = token.IndexOf(").") + 1;
            tokenLength = (tokenPrefix + ":").Length;
            if (chainIndex == 0) { // ")." has not be found
                return new Tuple<string, string>(token.Substring(tokenLength).Trim(new char[] { '(', ')' }), "");
            }
            if (!token.StartsWith((tokenPrefix + ":"), StringComparison.OrdinalIgnoreCase) || chainIndex <= tokenLength) {
                return null;
            }
            return new Tuple<string, string>(token.Substring(tokenLength, chainIndex - tokenLength).Trim(new char[] { '(', ')' }), token.Substring(chainIndex + 1));

        }
    }

}
