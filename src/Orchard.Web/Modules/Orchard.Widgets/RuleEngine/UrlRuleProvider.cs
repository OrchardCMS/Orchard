using System;
using System.Web;
using Orchard.Environment.Configuration;
using Orchard.Mvc;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.RuleEngine {
    public class UrlRuleProvider : IRuleProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ShellSettings _shellSettings;

        public UrlRuleProvider(IHttpContextAccessor httpContextAccessor, ShellSettings shellSettings) {
            _httpContextAccessor = httpContextAccessor;
            _shellSettings = shellSettings;
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

                if(!String.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                    appPath = String.Concat(appPath, "/", _shellSettings.RequestUrlPrefix);

                url = String.Concat(appPath, "/", url);
            }

            if (!url.Contains("?"))
                url = url.TrimEnd('/');

            var requestPath = context.Request.Path;
            if (!requestPath.Contains("?"))
                requestPath = requestPath.TrimEnd('/');

            ruleContext.Result = url.EndsWith("*")
                ? requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
                : string.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase);
        }
    }
}