using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PI.Authentication.Services
{
    public interface IPIOrchardUserService : IDependency
    {
        void SyncUserToOrchardUser(string userName, string password, string email, bool signin);
    }

}
