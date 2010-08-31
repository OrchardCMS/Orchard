using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Drivers;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Settings;
using Orchard.Messaging.Services;
using System.Collections;
using System.Collections.Generic;

namespace Orchard.Users.Services {
    [UsedImplicitly]
    public class MembershipService : IMembershipService {
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly IRepository<UserPartRecord> _userRepository;

        public MembershipService(IContentManager contentManager, IRepository<UserPartRecord> userRepository, IMessageManager messageManager, IEnumerable<IUserEventHandler> userEventHandlers) {
            _contentManager = contentManager;
            _userRepository = userRepository;
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
            user.Record.RegistrationStatus = registrationSettings.UsersAreModerated ? UserStatus.Pending : UserStatus.Approved;
            user.Record.EmailStatus = registrationSettings.UsersMustValidateEmail ? UserStatus.Pending : UserStatus.Approved;

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

            if ( registrationSettings.UsersMustValidateEmail ) {
                SendEmailValidationMessage(user);
            }

            if ( registrationSettings.UsersAreModerated && registrationSettings.NotifyModeration ) {
                var superUser = GetUser(CurrentSite.SuperUser);
                if(superUser != null)
                    _messageManager.Send(superUser.ContentItem.Record, MessageTypes.Moderation);
            }

            return user;
        }

        public void SendEmailValidationMessage(IUser user) {
            _messageManager.Send(user.ContentItem.Record, MessageTypes.Validation);
        }

        public IUser GetUser(string username) {
            var lowerName = username == null ? "" : username.ToLower();

            var userRecord = _userRepository.Get(x => x.NormalizedUserName == lowerName);
            if (userRecord == null) {
                return null;
            }
            return _contentManager.Get<IUser>(userRecord.Id);
        }

        public IUser ValidateUser(string userNameOrEmail, string password) {
            var lowerName = userNameOrEmail == null ? "" : userNameOrEmail.ToLower();

            var userRecord = _userRepository.Get(x => x.NormalizedUserName == lowerName);

            if(userRecord == null)
                userRecord = _userRepository.Get(x => x.Email == lowerName);

            if (userRecord == null || ValidatePassword(userRecord, password) == false)
                return null;

            if ( userRecord.EmailStatus != UserStatus.Approved )
                return null;

            if ( userRecord.RegistrationStatus != UserStatus.Approved )
                return null;

            return _contentManager.Get<IUser>(userRecord.Id);
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

        private bool ValidatePassword(UserPartRecord partRecord, string password) {
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
