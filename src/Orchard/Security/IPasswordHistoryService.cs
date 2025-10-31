using System.Collections.Generic;

namespace Orchard.Security
{
    public interface IPasswordHistoryService : IDependency
    {
        void CreateEntry(PasswordHistoryEntry context);
        IEnumerable<PasswordHistoryEntry> GetLastPasswords(IUser user, int count);
        bool PasswordMatchLastOnes(string Password, IUser user, int count);
    }
}
