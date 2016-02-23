using System;
using Orchard.Conditions.Services;
using Orchard.Environment.Configuration;
using Orchard.Mvc;

namespace Orchard.Conditions.Providers {
    public class UrlCondition : IConditionProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ShellSettings _shellSettings;

        public UrlCondition(IHttpContextAccessor httpContextAccessor, ShellSettings shellSettings) {
            _httpContextAccessor = httpContextAccessor;
            _shellSettings = shellSettings;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (!String.Equals(evaluationContext.FunctionName, "url", StringComparison.OrdinalIgnoreCase))
                return;

            var context = _httpContextAccessor.Current();
            var url = Convert.ToString(evaluationContext.Arguments[0]);
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

            evaluationContext.Result = url.EndsWith("*")
                ? requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
                : string.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase);
        }
    }
}