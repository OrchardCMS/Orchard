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

        public void Add(ShellSettings settings) {
            _shells = _shells
                .Where(s=>s.Name != settings.Name)
                .Concat(new[] { settings })
                .ToArray();
        }

        public IEnumerable<ShellSettings> List() {
            return _shells;
        }

        public ShellSettings Match(HttpContextBase httpContext) {
            return _shells.SingleOrDefault(settings => settings.Name == "Default");
        }
    }
}
