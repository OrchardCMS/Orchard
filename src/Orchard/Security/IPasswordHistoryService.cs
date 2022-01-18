using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Security;

namespace Orchard.Security {
    public interface IPasswordHistoryService : IDependency {
        void CreateEntry(PasswordHistoryEntry context);
        IEnumerable<PasswordHistoryEntry> GetLastPasswords(IUser user, int count);
        bool PasswordMatchLastOnes(string Password, IUser user, int count);
    }
}
