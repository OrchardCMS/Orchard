using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Messaging.Services;
using System.Collections.Generic;
using Orchard.Services;

namespace Orchard.Users.Services {
    [UsedImplicitly]
    public class MembershipService : IMembershipService {
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageManager _messageManager;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;

        public MembershipService(IOrchardServices orchardServices, IMessageManager messageManager, IEnumerable<IUserEventHandler> userEventHandlers, IClock clock) {
            _orchardServices = orchardServices;
            _messageManager = messageManager;
            _userEventHandlers = userEventHandlers;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public MembershipSettings GetSettings() {
            var settings = new MembershipSettings();
            // accepting defaults
            return settings;
        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            Logger.Information("CreateUser {0} {1}", createUserParams.Username, createUserParams.Email);

            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();

            var user = _orchardServices.ContentManager.New<UserPart>("User");

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

            _orchardServices.ContentManager.Create(user);

            foreach ( var userEventHandler in _userEventHandlers ) {
                userEventHandler.Created(userContext);
            }

            if ( registrationSettings != null  && registrationSettings.UsersAreModerated && registrationSettings.NotifyModeration && !createUserParams.IsApproved ) {
                var usernames = String.IsNullOrWhiteSpace(registrationSettings.NotificationsRecipients)
                                    ? new string[0]
                                    : registrationSettings.NotificationsRecipients.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

                foreach ( var userName in usernames ) {
                    if (String.IsNullOrWhiteSpace(userName)) {
                        continue;
                    }
                    var recipient = GetUser(userName);
                    if (recipient != null)
                        _messageManager.Send(recipient.ContentItem.Record, MessageTypes.Moderation, "email");
                }
            }

            return user;
        }

        public IUser GetUser(string username) {
            var lowerName = username == null ? "" : username.ToLower();

            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();
        }

        public IUser ValidateUser(string userNameOrEmail, string password) {
            var lowerName = userNameOrEmail == null ? "" : userNameOrEmail.ToLower();

            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();

            if(user == null)
                user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).List().FirstOrDefault();

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
                    throw new ApplicationException(T("Unexpected password format value").ToString());
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
            using (var random = new RNGCryptoServiceProvider()) {
                random.GetBytes(saltBytes);
            }

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            byte[] hashBytes;
            using (var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm)) {
                hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            }

            partRecord.PasswordFormat = MembershipPasswordFormat.Hashed;
            partRecord.Password = Convert.ToBase64String(hashBytes);
            partRecord.PasswordSalt = Convert.ToBase64String(saltBytes);
        }

        private static bool ValidatePasswordHashed(UserPartRecord partRecord, string password) {

            var saltBytes = Convert.FromBase64String(partRecord.PasswordSalt);

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            byte[] hashBytes;
            using (var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm)) {
                hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            }
            
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
