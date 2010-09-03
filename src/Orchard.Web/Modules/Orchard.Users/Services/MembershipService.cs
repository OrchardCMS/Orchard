using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Drivers;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Settings;
using Orchard.Messaging.Services;
using System.Collections.Generic;

namespace Orchard.Users.Services {
    [UsedImplicitly]
    public class MembershipService : IMembershipService {
        private static readonly TimeSpan DelayToValidate = new TimeSpan(7, 0, 0, 0); // one week to validate email
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;

        public MembershipService(IContentManager contentManager, IMessageManager messageManager, IEnumerable<IUserEventHandler> userEventHandlers) {
            _contentManager = contentManager;
            _messageManager = messageManager;
            _userEventHandlers = userEventHandlers;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public MembershipSettings GetSettings() {
            var settings = new MembershipSettings();
            // accepting defaults
            return settings;
        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            Logger.Information("CreateUser {0} {1}", createUserParams.Username, createUserParams.Email);

            var registrationSettings = CurrentSite.As<RegistrationSettingsPart>();

            var user = _contentManager.New<UserPart>(UserPartDriver.ContentType.Name);

            user.Record.UserName = createUserParams.Username;
            user.Record.Email = createUserParams.Email;
            user.Record.NormalizedUserName = createUserParams.Username.ToLower();
            user.Record.HashAlgorithm = "SHA1";
            SetPassword(user.Record, createUserParams.Password);

            if ( registrationSettings != null ) {
                user.Record.RegistrationStatus = registrationSettings.UsersAreModerated ? UserStatus.Pending : UserStatus.Approved;
                user.Record.EmailStatus = registrationSettings.UsersMustValidateEmail ? UserStatus.Pending : UserStatus.Approved;
            }

            if(createUserParams.IsApproved) {
                user.Record.RegistrationStatus = UserStatus.Approved;
                user.Record.EmailStatus = UserStatus.Approved;
            }

            var userContext = new UserContext {User = user, Cancel = false};
            foreach(var userEventHandler in _userEventHandlers) {
                userEventHandler.Creating(userContext);
            }

            if(userContext.Cancel) {
                return null;
            }

            _contentManager.Create(user);

            foreach ( var userEventHandler in _userEventHandlers ) {
                userEventHandler.Created(userContext);
            }

            if ( registrationSettings != null  && registrationSettings.UsersAreModerated && registrationSettings.NotifyModeration && !createUserParams.IsApproved ) {
                var superUser = GetUser(CurrentSite.SuperUser);
                if(superUser != null)
                    _messageManager.Send(superUser.ContentItem.Record, MessageTypes.Moderation);
            }

            return user;
        }

        public void SendChallengeEmail(IUser user, string url) {
            _messageManager.Send(user.ContentItem.Record, MessageTypes.Validation, "Email", new Dictionary<string, string> { { "ChallengeUrl", url } });
        }

        public IUser ValidateChallengeToken(string challengeToken) {
            string username;
            DateTime validateByUtc;

            if(!DecryptChallengeToken(challengeToken, out username, out validateByUtc)) {
                return null;
            }

            if ( validateByUtc < DateTime.UtcNow )
                return null;

            var user = GetUser(username);
            if ( user == null )
                return null;

            user.As<UserPart>().EmailStatus = UserStatus.Approved;

            return user;
        }

        public string GetEncryptedChallengeToken(IUser user) {
            var challengeToken = new XElement("Token", new XAttribute("username", user.UserName), new XAttribute("validate-by-utc", DateTime.UtcNow.Add(DelayToValidate).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return MachineKey.Encode(data, MachineKeyProtection.All);
        }

        private static bool DecryptChallengeToken(string challengeToken, out string username, out DateTime validateByUtc) {
            username = null;
            validateByUtc = DateTime.UtcNow;

            try {
                var data = MachineKey.Decode(challengeToken, MachineKeyProtection.All);
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                username = element.Attribute("username").Value;
                validateByUtc = DateTime.Parse(element.Attribute("validate-by-utc").Value, CultureInfo.InvariantCulture);
                return true;
            }
            catch {
                return false;
            }

        }

        public IUser GetUser(string username) {
            var lowerName = username == null ? "" : username.ToLower();

            return _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();
        }

        public IUser ValidateUser(string userNameOrEmail, string password) {
            var lowerName = userNameOrEmail == null ? "" : userNameOrEmail.ToLower();

            var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();

            if(user == null)
                user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).List().FirstOrDefault();

            if ( user == null || ValidatePassword(user.As<UserPart>().Record, password) == false )
                return null;

            if ( user.EmailStatus != UserStatus.Approved )
                return null;

            if ( user.RegistrationStatus != UserStatus.Approved )
                return null;

            return user;
        }

        public void SetPassword(IUser user, string password) {
            if (!user.Is<UserPart>())
                throw new InvalidCastException();

            var userRecord = user.As<UserPart>().Record;
            SetPassword(userRecord, password);
        }


        void SetPassword(UserPartRecord partRecord, string password) {
            switch (GetSettings().PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    SetPasswordClear(partRecord, password);
                    break;
                case MembershipPasswordFormat.Hashed:
                    SetPasswordHashed(partRecord, password);
                    break;
                case MembershipPasswordFormat.Encrypted:
                    SetPasswordEncrypted(partRecord, password);
                    break;
                default:
                    throw new ApplicationException("Unexpected password format value");
            }
        }

        private static bool ValidatePassword(UserPartRecord partRecord, string password) {
            // Note - the password format stored with the record is used
            // otherwise changing the password format on the site would invalidate
            // all logins
            switch (partRecord.PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    return ValidatePasswordClear(partRecord, password);
                case MembershipPasswordFormat.Hashed:
                    return ValidatePasswordHashed(partRecord, password);
                case MembershipPasswordFormat.Encrypted:
                    return ValidatePasswordEncrypted(partRecord, password);
                default:
                    throw new ApplicationException("Unexpected password format value");
            }
        }

        private static void SetPasswordClear(UserPartRecord partRecord, string password) {
            partRecord.PasswordFormat = MembershipPasswordFormat.Clear;
            partRecord.Password = password;
            partRecord.PasswordSalt = null;
        }

        private static bool ValidatePasswordClear(UserPartRecord partRecord, string password) {
            return partRecord.Password == password;
        }

        private static void SetPasswordHashed(UserPartRecord partRecord, string password) {

            var saltBytes = new byte[0x10];
            var random = new RNGCryptoServiceProvider();
            random.GetBytes(saltBytes);

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm);
            var hashBytes = hashAlgorithm.ComputeHash(combinedBytes);

            partRecord.PasswordFormat = MembershipPasswordFormat.Hashed;
            partRecord.Password = Convert.ToBase64String(hashBytes);
            partRecord.PasswordSalt = Convert.ToBase64String(saltBytes);
        }

        private static bool ValidatePasswordHashed(UserPartRecord partRecord, string password) {

            var saltBytes = Convert.FromBase64String(partRecord.PasswordSalt);

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm);
            var hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            
            return partRecord.Password == Convert.ToBase64String(hashBytes);
        }

        private static void SetPasswordEncrypted(UserPartRecord partRecord, string password) {
            throw new NotImplementedException();
        }

        private static bool ValidatePasswordEncrypted(UserPartRecord partRecord, string password) {
            throw new NotImplementedException();
        }

    }
}
