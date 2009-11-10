using System;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Users.Models {
    public sealed class UserModel : ModelPartWithRecord<UserRecord>, IUser {
        string IUser.UserName {get { return Record.UserName; }}
        string IUser.Email {get { return Record.Email; }}
    }
}
