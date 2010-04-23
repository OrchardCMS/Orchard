using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Orchard.Environment.Configuration;

namespace Orchard.Environment {
    public interface IRunningShellTable {
        void Add(ShellSettings settings);
        IEnumerable<ShellSettings> List();
        ShellSettings Match(HttpContextBase httpContext);
    }

    public class RunningShellTable : IRunningShellTable {
        private IEnumerable<ShellSettings> _shells = Enumerable.Empty<ShellSettings>();
        private IEnumerable<IGrouping<string, ShellSettings>> _shellsByHost = Enumerable.Empty<ShellSettings>().GroupBy(x => default(string));
        private ShellSettings _fallback;

        public void Add(ShellSettings settings) {
            _shells = _shells
                .Where(s => s.Name != settings.Name)
                .Concat(new[] { settings })
                .ToArray();

            var qualified =
                _shells.Where(x => !string.IsNullOrEmpty(x.RequestUrlHost) || !string.IsNullOrEmpty(x.RequestUrlPrefix));

            var unqualified =
                _shells.Where(x => string.IsNullOrEmpty(x.RequestUrlHost) && string.IsNullOrEmpty(x.RequestUrlPrefix));

            _shellsByHost = qualified
                .GroupBy(s => s.RequestUrlHost ?? "")
                .OrderByDescending(g => g.Key.Length);

            if (unqualified.Count() == 1) {
                // only one shell had no request url criteria
                _fallback = unqualified.Single();
            }
            else if (unqualified.Any()) {
                // two or more shells had no request criteria. 
                // this is technically a misconfiguration - so fallback to the default shell
                // if it's one which will catch all requests
                _fallback = unqualified.SingleOrDefault(x => x.Name == "Default");
            }
            else {
                // no shells are unqualified - a request that does not match a shell's spec
                // will not be mapped to routes coming from orchard
                _fallback = null;
            }
        }

        public IEnumerable<ShellSettings> List() {
            return _shells;
        }

        public ShellSettings Match(HttpContextBase httpContext) {
            var host = httpContext.Request.ServerVariables.Get("HTTP_HOST") ?? "";

            var hostLength = host.IndexOf(':');
            if (hostLength != -1)
                host = host.Substring(0, hostLength);

            var appRelativePath = httpContext.Request.AppRelativeCurrentExecutionFilePath;

            var mostQualifiedMatch = _shellsByHost
                .Where(group => host.EndsWith(group.Key, StringComparison.OrdinalIgnoreCase))
                .SelectMany(group => group
                    .OrderByDescending(settings => (settings.RequestUrlPrefix ?? "").Length))
                    .Where(settings => appRelativePath.StartsWith(settings.RequestUrlPrefix ?? "", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

            return mostQualifiedMatch ?? _fallback;
        }

    }
}
