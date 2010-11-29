using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.Security;
using System.Xml.Linq;
using Orchard.Services;
using System.Globalization;
using System.Text;
using System.Web.Security;
using Orchard.Messaging.Services;
using Orchard.Environment.Configuration;

namespace Orchard.Users.Services {
    [UsedImplicitly]
    public class UserService : IUserService {
        private static readonly TimeSpan DelayToValidate = new TimeSpan(7, 0, 0, 0); // one week to validate email
        private static readonly TimeSpan DelayToResetPassword = new TimeSpan(1, 0, 0, 0); // 24 hours to validate email

        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IClock _clock;
        private readonly IMessageManager _messageManager;
        private readonly ShellSettings _shellSettings;

        public UserService(IContentManager contentManager, IMembershipService membershipService, IClock clock, IMessageManager messageManager, ShellSettings shellSettings) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _clock = clock;
            _messageManager = messageManager;
            _shellSettings = shellSettings;
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

        public string CreateNonce(IUser user, TimeSpan delay) {
            // the tenant's name is added to the token to prevent cross-tenant requests
            var challengeToken = new XElement("n", new XAttribute("s", _shellSettings.Name), new XAttribute("un", user.UserName), new XAttribute("utc", _clock.UtcNow.ToUniversalTime().Add(delay).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.Unicode.GetBytes(challengeToken);
            return MachineKey.Encode(data, MachineKeyProtection.All);
        }

        public bool DecryptNonce(string challengeToken, out string username, out DateTime validateByUtc) {
            username = null;
            validateByUtc = _clock.UtcNow;

            try {
                var data = MachineKey.Decode(challengeToken, MachineKeyProtection.All);
                var xml = Encoding.Unicode.GetString(data);
                var element = XElement.Parse(xml);
                var tenant = element.Attribute("s").Value;
                username = element.Attribute("un").Value;
                validateByUtc = DateTime.Parse(element.Attribute("utc").Value, CultureInfo.InvariantCulture);
                return String.Equals(_shellSettings.Name, tenant, StringComparison.Ordinal) && _clock.UtcNow <= validateByUtc;
            }
            catch {
                return false;
            }

        }

        public IUser ValidateChallenge(string nonce) {
            string username;
            DateTime validateByUtc;

            if (!DecryptNonce(nonce, out username, out validateByUtc)) {
                return null;
            }

            if (validateByUtc < _clock.UtcNow)
                return null;

            var user = _membershipService.GetUser(username);
            if (user == null)
                return null;

            user.As<UserPart>().EmailStatus = UserStatus.Approved;

            return user;
        }

        public void SendChallengeEmail(IUser user, Func<string, string> createUrl) {
            string nonce = CreateNonce(user, DelayToValidate);
            string url = createUrl(nonce);
            _messageManager.Send(user.ContentItem.Record, MessageTypes.Validation, "email", new Dictionary<string, string> { { "ChallengeUrl", url } });
        }

        public bool SendLostPasswordEmail(string usernameOrEmail, Func<string, string> createUrl) {
            var lowerName = usernameOrEmail.ToLower();
            var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == lowerName).List().FirstOrDefault();

            if (user != null) {
                string nonce = CreateNonce(user, DelayToResetPassword);
                string url = createUrl(nonce);

                _messageManager.Send(user.ContentItem.Record, MessageTypes.LostPassword, "email", new Dictionary<string, string> { { "LostPasswordUrl", url } });
                return true;
            }

            return false;
        }

        public IUser ValidateLostPassword(string nonce) {
            string username;
            DateTime validateByUtc;

            if (!DecryptNonce(nonce, out username, out validateByUtc)) {
                return null;
            }

            if (validateByUtc < _clock.UtcNow)
                return null;

            var user = _membershipService.GetUser(username);
            if (user == null)
                return null;

            return user;
        }
    }
}