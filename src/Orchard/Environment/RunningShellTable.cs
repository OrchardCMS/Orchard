using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Configuration;

namespace Orchard.Environment {
    public interface IRunningShellTable {
        void Add(ShellSettings settings);
        void Remove(ShellSettings settings);
        void Update(ShellSettings settings);
        ShellSettings Match(HttpContextBase httpContext);
        ShellSettings Match(string host, string appRelativeCurrentExecutionFilePath);
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

            Organize();
        }

        public void Remove(ShellSettings settings) {
            _shells = _shells
                .Where(s => s.Name != settings.Name)
                .ToArray();

            Organize();
        }

        public void Update(ShellSettings settings) {
            _shells = _shells
                .Where(s => s.Name != settings.Name)
                .ToArray();

            _shells = _shells
                .Concat(new[] { settings })
                .ToArray();

            Organize();
        }

        private void Organize() {
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
                _fallback = unqualified.SingleOrDefault(x => x.Name == ShellSettings.DefaultName);
            }
            else {
                // no shells are unqualified - a request that does not match a shell's spec
                // will not be mapped to routes coming from orchard
                _fallback = null;
            }
        }

        public ShellSettings Match(HttpContextBase httpContext) {
            return Match(httpContext.Request.Headers["Host"], httpContext.Request.AppRelativeCurrentExecutionFilePath);
        }

        public ShellSettings Match(string host, string appRelativePath) {
            // use Host header to prevent proxy alteration of the orignal request
            var hostLength = host.IndexOf(':');
            if (hostLength != -1)
                host = host.Substring(0, hostLength);

            var mostQualifiedMatch = _shellsByHost
                .Where(group => host.EndsWith(group.Key, StringComparison.OrdinalIgnoreCase))
                .SelectMany(group => group
                    .OrderByDescending(settings => (settings.RequestUrlPrefix ?? "").Length))
                    .Where(settings => settings.State.CurrentState != TenantState.State.Disabled && appRelativePath.StartsWith(settings.RequestUrlPrefix ?? "", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

            return mostQualifiedMatch ?? _fallback;
        }
    }
}
