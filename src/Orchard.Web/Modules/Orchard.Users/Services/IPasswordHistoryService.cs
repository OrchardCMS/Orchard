using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public interface IPasswordHistoryService : IDependency{
        void CreateEntry(UserPart user);
        bool MatchLastPasswords(string newPassword, int howManyPasswords, UserPart user);
    }
}
