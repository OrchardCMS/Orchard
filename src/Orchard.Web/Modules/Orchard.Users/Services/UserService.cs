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
        private readonly IEncryptionService _encryptionService;

        public UserService(IContentManager contentManager, IMembershipService membershipService, IClock clock, IMessageManager messageManager, ShellSettings shellSettings, IEncryptionService encryptionService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _clock = clock;
            _messageManager = messageManager;
            _encryptionService = encryptionService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyUserUnicity(string userName, string email) {
            string normalizedUserName = userName.ToLowerInvariant();

            if (_contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user => 
                                          user.NormalizedUserName == normalizedUserName || 
                                          user.Email == email)
                                   .List().Any()) {
                return false;
            }

            return true;
        }

        public bool VerifyUserUnicity(int id, string userName, string email) {
            string normalizedUserName = userName.ToLowerInvariant();

            if (_contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user =>
                                          user.NormalizedUserName == normalizedUserName ||
                                          user.Email == email)
                                   .List().Any(user => user.Id != id)) {
                return false;
            }

            return true;
        }

        public string CreateNonce(IUser user, TimeSpan delay) {
            var challengeToken = new XElement("n", new XAttribute("un", user.UserName), new XAttribute("utc", _clock.UtcNow.ToUniversalTime().Add(delay).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return Convert.ToBase64String(_encryptionService.Encode(data));
        }

        public bool DecryptNonce(string nonce, out string username, out DateTime validateByUtc) {
            username = null;
            validateByUtc = _clock.UtcNow;

            try {
                var data = _encryptionService.Decode(Convert.FromBase64String(nonce));
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                username = element.Attribute("un").Value;
                validateByUtc = DateTime.Parse(element.Attribute("utc").Value, CultureInfo.InvariantCulture);
                return _clock.UtcNow <= validateByUtc;
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
            var lowerName = usernameOrEmail.ToLowerInvariant();
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