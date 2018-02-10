using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Events;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;
using System.Web.Security;

namespace Orchard.Users.Services {
    [OrchardSuppressDependency("Orchard.Security.NullMembershipService")]
    public class MembershipService : IMembershipService {
        private const string PBKDF2 = "PBKDF2";
        private const string DefaultHashAlgorithm = PBKDF2;

        private readonly IOrchardServices _orchardServices;
        private readonly IMessageService _messageService;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly IEncryptionService _encryptionService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IAppConfigurationAccessor _appConfigurationAccessor;
        private readonly IClock _clock;

        public MembershipService(
            IOrchardServices orchardServices,
            IMessageService messageService,
            IUserEventHandler userEventHandlers,
            IClock clock,
            IEncryptionService encryptionService,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            IAppConfigurationAccessor appConfigurationAccessor) {
            _orchardServices = orchardServices;
            _messageService = messageService;
            _userEventHandlers = userEventHandlers;
            _encryptionService = encryptionService;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _appConfigurationAccessor = appConfigurationAccessor;
            _clock = clock;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IMembershipSettings GetSettings() {
            return _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            Logger.Information("CreateUser {0} {1}", createUserParams.Username, createUserParams.Email);

            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();

            var user = _orchardServices.ContentManager.New<UserPart>("User");

            user.UserName = createUserParams.Username;
            user.Email = createUserParams.Email;
            user.NormalizedUserName = createUserParams.Username.ToLowerInvariant();
            user.HashAlgorithm = PBKDF2;
            user.CreatedUtc = _clock.UtcNow;
            SetPassword(user, createUserParams.Password);

            if ( registrationSettings != null) {
                user.RegistrationStatus = registrationSettings.UsersAreModerated ? UserStatus.Pending : UserStatus.Approved;
                user.EmailStatus = registrationSettings.UsersMustValidateEmail ? UserStatus.Pending : UserStatus.Approved;
            }

            if(createUserParams.IsApproved) {
                user.RegistrationStatus = UserStatus.Approved;
                user.EmailStatus = UserStatus.Approved;
            }

            var userContext = new UserContext { User = user, Cancel = false, UserParameters = createUserParams };
            _userEventHandlers.Creating(userContext);

            if(userContext.Cancel) {
                return null;
            }

            _orchardServices.ContentManager.Create(user);

            _userEventHandlers.Created(userContext);
            if (user.RegistrationStatus == UserStatus.Approved) {
                _userEventHandlers.Approved(user);
            }

            if ( registrationSettings != null
                && registrationSettings.UsersAreModerated
                && registrationSettings.NotifyModeration
                && !createUserParams.IsApproved) {
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
                            {"Subject", T("New account on {0}.", _orchardServices.WorkContext.CurrentSite.SiteName).Text},
                            {"Body", _shapeDisplay.Display(template)},
                            {"Recipients", recipient.Email }
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

        public IUser ValidateUser(string userNameOrEmail, string password, out List<LocalizedString> validationErrors) {
            var lowerName = userNameOrEmail == null ? "" : userNameOrEmail.ToLowerInvariant();
            validationErrors = new List<LocalizedString>();
            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();
            if (user == null)
                user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).List().FirstOrDefault();

            if (user == null || ValidatePassword(user.As<UserPart>(), password) == false) {
                validationErrors.Add(T("The username or e-mail or password provided is incorrect."));
                return null;
            }

            if (user.EmailStatus != UserStatus.Approved)
                validationErrors.Add(T("You must verify your email"));

            if (user.RegistrationStatus != UserStatus.Approved)
                validationErrors.Add(T("You must be approved before being able to login"));

            return user;
        }

        public bool PasswordIsExpired(IUser user, int days) {
            return user.As<UserPart>().LastPasswordChangeUtc.Value.AddDays(days) < _clock.UtcNow;
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
            userPart.LastPasswordChangeUtc = _clock.UtcNow;
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

            userPart.PasswordFormat = MembershipPasswordFormat.Hashed;
            userPart.Password = ComputeHashBase64(userPart.HashAlgorithm, saltBytes, password);
            userPart.PasswordSalt = Convert.ToBase64String(saltBytes);
        }

        private bool ValidatePasswordHashed(UserPart userPart, string password) {
            var saltBytes = Convert.FromBase64String(userPart.PasswordSalt);

            bool isValid;
            if (userPart.HashAlgorithm == PBKDF2) {
                // We can't reuse ComputeHashBase64 as the internally generated salt repeated calls to Crypto.HashPassword() return different results.
                isValid = Crypto.VerifyHashedPassword(userPart.Password, Encoding.Unicode.GetString(CombineSaltAndPassword(saltBytes, password)));
            }
            else {
                isValid = SecureStringEquality(userPart.Password, ComputeHashBase64(userPart.HashAlgorithm, saltBytes, password));
            }

            // Migrating older password hashes to Default algorithm if necessary and enabled.
            if (isValid && userPart.HashAlgorithm != DefaultHashAlgorithm) {
                var keepOldConfiguration = _appConfigurationAccessor.GetConfiguration("Orchard.Users.KeepOldPasswordHash");
                if (String.IsNullOrEmpty(keepOldConfiguration) || keepOldConfiguration.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                    userPart.HashAlgorithm = DefaultHashAlgorithm;
                    userPart.Password = ComputeHashBase64(userPart.HashAlgorithm, saltBytes, password);
                }
            }

            return isValid;
        }

        private static string ComputeHashBase64(string hashAlgorithmName, byte[] saltBytes, string password) {
            var combinedBytes = CombineSaltAndPassword(saltBytes, password);

            // Extending HashAlgorithm would be too complicated: http://stackoverflow.com/questions/6460711/adding-a-custom-hashalgorithmtype-in-c-sharp-asp-net?lq=1
            if (hashAlgorithmName == PBKDF2) {
                // HashPassword() already returns a base64 string.
                return Crypto.HashPassword(Encoding.Unicode.GetString(combinedBytes));
            }
            else {
                using (var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName)) {
                    return Convert.ToBase64String(hashAlgorithm.ComputeHash(combinedBytes));
                }
            }
        }

        /// <summary>
        /// Compares two strings without giving hint about the time it takes to do so.
        /// </summary>
        /// <param name="a">The first string to compare.</param>
        /// <param name="b">The second string to compare.</param>
        /// <returns><c>true</c> if both strings are equal, <c>false</c>.</returns>
        private bool SecureStringEquality(string a, string b) {
            if (a == null || b == null || (a.Length != b.Length)) {
                return false;
            }

            var aBytes = Encoding.Unicode.GetBytes(a);
            var bBytes = Encoding.Unicode.GetBytes(b);

            var bytesAreEqual = true;
            for (int i = 0; i < a.Length; i++) {
                bytesAreEqual &= (aBytes[i] == bBytes[i]);
            }

            return bytesAreEqual;
        }

        private static byte[] CombineSaltAndPassword(byte[] saltBytes, string password) {
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            return saltBytes.Concat(passwordBytes).ToArray();
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
