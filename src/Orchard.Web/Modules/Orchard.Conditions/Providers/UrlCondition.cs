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
            if (!string.Equals(evaluationContext.FunctionName, "url", StringComparison.OrdinalIgnoreCase))
                return;

            var context = _httpContextAccessor.Current();
            foreach (var argument in evaluationContext.Arguments) {
                var url = Convert.ToString(argument);
                if (url.StartsWith("~/")) {
                    url = url.Substring(2);
                    var appPath = context.Request.ApplicationPath;
                    if (appPath == "/")
                        appPath = "";

                    if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                        appPath = string.Concat(appPath, "/", _shellSettings.RequestUrlPrefix);

                    url = string.Concat(appPath, "/", url);
                }

                if (!url.Contains("?"))
                    url = url.TrimEnd('/');

                var requestPath = context.Request.Path;
                if (!requestPath.Contains("?"))
                    requestPath = requestPath.TrimEnd('/');

                if ((url.EndsWith("*") && requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)) ||
                    string.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase)) {
                    evaluationContext.Result = true;
                    return;
                }
            }

            evaluationContext.Result = false;
        }
    }
}