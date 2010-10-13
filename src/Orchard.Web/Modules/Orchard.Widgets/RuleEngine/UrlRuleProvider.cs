using System;
using Orchard.Mvc;
using Orchard.UI.Widgets;

namespace Orchard.Widgets.RuleEngine {
    public class UrlRuleProvider : IRuleProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlRuleProvider(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Process(RuleContext ruleContext) {
            if (!String.Equals(ruleContext.FunctionName, "url", StringComparison.OrdinalIgnoreCase))
                return;

            var context = _httpContextAccessor.Current();
            var url = Convert.ToString(ruleContext.Arguments[0]);
            if (url.StartsWith("~/")) {
                url = url.Substring(2);
                var appPath = context.Request.ApplicationPath;
                if (appPath == "/")
                    appPath = "";
                url = string.Format("{0}/{1}", appPath, url);
            }
            if (!url.Contains("?") && url.EndsWith("/"))
                url = url.TrimEnd('/');
            ruleContext.Result = url.EndsWith("*")
                                     ? context.Request.RawUrl.ToUpperInvariant().StartsWith(url.ToUpperInvariant())
                                     : context.Request.Path.ToUpperInvariant() == url.ToUpperInvariant();
        }
    }
}