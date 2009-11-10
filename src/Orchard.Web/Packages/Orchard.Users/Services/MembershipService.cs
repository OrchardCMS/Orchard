using System;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class MembershipService : IMembershipService {
        private readonly IModelManager _modelManager;

        public MembershipService(IModelManager modelManager) {
            _modelManager = modelManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void ReadSettings(MembershipSettings settings) {
            // accepting defaults
        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            Logger.Information("CreateUser {0} {1}", createUserParams.Username, createUserParams.Email);
            var user = _modelManager.New("user");
            user.As<UserModel>().Record = new UserRecord {
                UserName = createUserParams.Username,
                Email = createUserParams.Email
            };
            _modelManager.Create(user);
            return user.As<IUser>();
        }

        public IUser GetUser(string username) {
            throw new NotImplementedException();
        }
    }
}
