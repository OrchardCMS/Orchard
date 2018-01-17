using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Environment.Configuration;

namespace Orchard.Security.Providers {
    public class TenantNameUserDataProvider : BaseUserDataProvider {

        private readonly ShellSettings _settings;

        public TenantNameUserDataProvider(
            ShellSettings settings) : base(false) {

            _settings = settings;
        }

        protected override string Value(IUser user) {
            return _settings.Name;
        }
    }
}
