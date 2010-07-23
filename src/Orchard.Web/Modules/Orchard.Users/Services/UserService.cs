using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    [UsedImplicitly]
    public class UserService : IUserService {
        private readonly IContentManager _contentManager;

        public UserService(IContentManager contentManager) {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public string VerifyUserUnicity(string userName, string email) {
            IEnumerable<UserPart> allUsers = _contentManager.Query<UserPart, UserPartRecord>().List();

            foreach (var user in allUsers) {
                if (String.Equals(userName.ToLower(), user.NormalizedUserName, StringComparison.OrdinalIgnoreCase)) {
                    return "A user with that name already exists";
                }
                if (String.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase)) {
                    return "A user with that email already exists";
                }
            }

            return null;
        }

        public string VerifyUserUnicity(int id, string userName, string email) {
            IEnumerable<UserPart> allUsers = _contentManager.Query<UserPart, UserPartRecord>().List();
            foreach (var user in allUsers) {
                if (user.Id == id)
                    continue;
                if (String.Equals(userName.ToLower(), user.NormalizedUserName, StringComparison.OrdinalIgnoreCase)) {
                    return "A user with that name already exists";
                }
                if (String.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase)) {
                    return "A user with that email already exists";
                }
            }
            return null;
        }
    }
}