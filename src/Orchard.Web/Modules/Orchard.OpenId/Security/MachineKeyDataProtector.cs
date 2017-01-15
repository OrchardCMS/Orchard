using System.Web.Security;
using Microsoft.Owin.Security.DataProtection;

namespace Orchard.OpenId.Security {
    public class MachineKeyProtectionProvider : IDataProtectionProvider {
        public IDataProtector Create(params string[] purposes) {
            return new MachineKeyDataProtector(purposes);
        }
    }

    public class MachineKeyDataProtector : IDataProtector {
        private readonly string[] _purposes;

        public MachineKeyDataProtector(string[] purposes) {
            _purposes = purposes;
        }

        public byte[] Protect(byte[] userData) {
            return MachineKey.Protect(userData, _purposes);
        }

        public byte[] Unprotect(byte[] protectedData) {
            return MachineKey.Unprotect(protectedData, _purposes);
        }
    }
}