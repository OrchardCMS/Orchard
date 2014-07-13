using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using JetBrains.Annotations;
using Orchard.DisplayManagement;
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
        private readonly IMessageService _messageService;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly IEncryptionService _encryptionService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;

        public MembershipService(
            IOrchardServices orchardServices, 
            IMessageService messageService, 
            IEnumerable<IUserEventHandler> userEventHandlers, 
            IClock clock, 
            IEncryptionService encryptionService,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay) {
            _orchardServices = orchardServices;
            _messageService = messageService;
            _userEventHandlers = userEventHandlers;
            _encryptionService = encryptionService;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
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

            user.UserName = createUserParams.Username;
            user.Email = createUserParams.Email;
            user.NormalizedUserName = createUserParams.Username.ToLowerInvariant();
            user.HashAlgorithm = "SHA1";
            SetPassword(user, createUserParams.Password);

            if ( registrationSettings != null ) {
                user.RegistrationStatus = registrationSettings.UsersAreModerated ? UserStatus.Pending : UserStatus.Approved;
                user.EmailStatus = registrationSettings.UsersMustValidateEmail ? UserStatus.Pending : UserStatus.Approved;
            }

            if(createUserParams.IsApproved) {
                user.RegistrationStatus = UserStatus.Approved;
                user.EmailStatus = UserStatus.Approved;
            }

            var userContext = new UserContext {User = user, Cancel = false, UserParameters = createUserParams};
            foreach(var userEventHandler in _userEventHandlers) {
                userEventHandler.Creating(userContext);
            }

            if(userContext.Cancel) {
                return null;
            }

            _orchardServices.ContentManager.Create(user);

            foreach ( var userEventHandler in _userEventHandlers ) {
                userEventHandler.Created(userContext);
                if (user.RegistrationStatus == UserStatus.Approved) {
                    userEventHandler.Approved(user);
                }
            }

            if ( registrationSettings != null  
                && registrationSettings.UsersAreModerated 
                && registrationSettings.NotifyModeration 
                && !createUserParams.IsApproved ) {
                var usernames = String.IsNullOrWhiteSpace(registrationSettings.NotificationsRecipients)
                                    ? new string[0]
                                    : registrationSettings.NotificationsRecipients.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

                foreach ( var userName in usernames ) {
                    if (String.IsNullOrWhiteSpace(userName)) {
                        continue;
                    }
                    var recipient = GetUser(userName);
                    if (recipient != null) {
                        var template = _shapeFactory.Create("Template_User_Moderated", Arguments.From(createUserParams));
                        template.Metadata.Wrappers.Add("Template_User_Wrapper");

                        var parameters = new Dictionary<string, object> {
                            {"Subject", T("New account").Text},
                            {"Body", _shapeDisplay.Display(template)},
                            {"Recipients", new [] { recipient.Email }}
                        };

                        _messageService.Send("Email", parameters);
                    }
                }
            }

            return user;
        }

        public IUser GetUser(string username) {
            var lowerName = username == null ? "" : username.ToLowerInvariant();

            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();
        }

        public IUser ValidateUser(string userNameOrEmail, string password) {
            var lowerName = userNameOrEmail == null ? "" : userNameOrEmail.ToLowerInvariant();

            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();

            if (user == null)
                user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).List().FirstOrDefault();

            if ( user == null || ValidatePassword(user.As<UserPart>(), password) == false )
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

            var userPart = user.As<UserPart>();

            switch (GetSettings().PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    SetPasswordClear(userPart, password);
                    break;
                case MembershipPasswordFormat.Hashed:
                    SetPasswordHashed(userPart, password);
                    break;
                case MembershipPasswordFormat.Encrypted:
                    SetPasswordEncrypted(userPart, password);
                    break;
                default:
                    throw new ApplicationException(T("Unexpected password format value").ToString());
            }
        }

        private bool ValidatePassword(UserPart userPart, string password) {
            // Note - the password format stored with the record is used
            // otherwise changing the password format on the site would invalidate
            // all logins
            switch (userPart.PasswordFormat) {
                case MembershipPasswordFormat.Clear:
                    return ValidatePasswordClear(userPart, password);
                case MembershipPasswordFormat.Hashed:
                    return ValidatePasswordHashed(userPart, password);
                case MembershipPasswordFormat.Encrypted:
                    return ValidatePasswordEncrypted(userPart, password);
                default:
                    throw new ApplicationException("Unexpected password format value");
            }
        }

        private static void SetPasswordClear(UserPart userPart, string password) {
            userPart.PasswordFormat = MembershipPasswordFormat.Clear;
            userPart.Password = password;
            userPart.PasswordSalt = null;
        }

        private static bool ValidatePasswordClear(UserPart userPart, string password) {
            return userPart.Password == password;
        }

        private static void SetPasswordHashed(UserPart userPart, string password) {

            var saltBytes = new byte[0x10];
            using (var random = new RNGCryptoServiceProvider()) {
                random.GetBytes(saltBytes);
            }

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            byte[] hashBytes;
            using (var hashAlgorithm = HashAlgorithm.Create(userPart.HashAlgorithm)) {
                hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            }

            userPart.PasswordFormat = MembershipPasswordFormat.Hashed;
            userPart.Password = Convert.ToBase64String(hashBytes);
            userPart.PasswordSalt = Convert.ToBase64String(saltBytes);
        }

        private static bool ValidatePasswordHashed(UserPart userPart, string password) {

            var saltBytes = Convert.FromBase64String(userPart.PasswordSalt);

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            byte[] hashBytes;
            using (var hashAlgorithm = HashAlgorithm.Create(userPart.HashAlgorithm)) {
                hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            }

            return userPart.Password == Convert.ToBase64String(hashBytes);
        }

        private void SetPasswordEncrypted(UserPart userPart, string password) {
            userPart.Password = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(password)));
            userPart.PasswordSalt = null;
            userPart.PasswordFormat = MembershipPasswordFormat.Encrypted;
        }

        private bool ValidatePasswordEncrypted(UserPart userPart, string password) {
            return String.Equals(password, Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(userPart.Password))), StringComparison.Ordinal);
        }
    }
}
