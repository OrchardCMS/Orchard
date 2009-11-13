using System;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class MembershipService : IMembershipService {
        private readonly IModelManager _modelManager;
        private readonly IRepository<UserRecord> _userRepository;

        public MembershipService(IModelManager modelManager, IRepository<UserRecord> userRepository) {
            _modelManager = modelManager;
            _userRepository = userRepository;
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
            var userRecord = _userRepository.Get(x => x.UserName == username);
            if (userRecord == null) {
                return null;
            }
            return _modelManager.Get(userRecord.Id).As<IUser>();
        }

        public IUser ValidateUser(string username, string password) {
            return GetUser(username);
        }
    }
}
