using Orchard.Environment.Configuration;

namespace Orchard.Security.Providers {
    public class TenantNameUserDataProvider : BaseUserDataProvider {

        private readonly ShellSettings _settings;

        public TenantNameUserDataProvider(
            ShellSettings settings) : base(false) {

            _settings = settings;
        }

        public override string Key {
            get { return "TenantName"; }
        }

        protected override string Value(IUser user) {
            return _settings.Name;
        }
    }
}
