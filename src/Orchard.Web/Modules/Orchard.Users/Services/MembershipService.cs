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
        private readonly IPasswordService _passwordService;
        private readonly IClock _clock;

        public MembershipService(
            IOrchardServices orchardServices,
            IMessageService messageService,
            IUserEventHandler userEventHandlers,
            IClock clock,
            IEncryptionService encryptionService,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            IAppConfigurationAccessor appConfigurationAccessor,
            IPasswordService passwordService) {
            _orchardServices = orchardServices;
            _messageService = messageService;
            _userEventHandlers = userEventHandlers;
            _encryptionService = encryptionService;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _appConfigurationAccessor = appConfigurationAccessor;
            _passwordService = passwordService;
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
            user.ForcePasswordChange = createUserParams.ForcePasswordChange;
            SetPassword(user, createUserParams.Password);

            if (registrationSettings != null) {
                user.RegistrationStatus = registrationSettings.UsersAreModerated ? UserStatus.Pending : UserStatus.Approved;
                user.EmailStatus = registrationSettings.UsersMustValidateEmail ? UserStatus.Pending : UserStatus.Approved;
            }

            if (createUserParams.IsApproved) {
                user.RegistrationStatus = UserStatus.Approved;
                user.EmailStatus = UserStatus.Approved;
            }

            var userContext = new UserContext { User = user, Cancel = false, UserParameters = createUserParams };
            _userEventHandlers.Creating(userContext);

            if (userContext.Cancel) {
                return null;
            }

            _orchardServices.ContentManager.Create(user);

            _userEventHandlers.Created(userContext);
            if (user.RegistrationStatus == UserStatus.Approved) {
                _userEventHandlers.Approved(user);
            }

            if (registrationSettings != null
                && registrationSettings.UsersAreModerated
                && registrationSettings.NotifyModeration
                && !createUserParams.IsApproved) {
                var usernames = String.IsNullOrWhiteSpace(registrationSettings.NotificationsRecipients)
                                    ? new string[0]
                                    : registrationSettings.NotificationsRecipients.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var userName in usernames) {
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

            if (user == null || !_passwordService.IsMatch(new PasswordContext {
                Password = user.Password,
                HashAlgorithm = user.HashAlgorithm,
                PasswordFormat = user.PasswordFormat,
                PasswordSalt = user.PasswordSalt
            }, password)) {
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
            // TODO: add providers to extend this

            // Null check on LastPasswordChangeUtc.
            // If this is null, use CreatedUtc as if it's the last password change date.
            // If both are null, consider the password to be expired.
            var passwordIsExpired = true;
            DateTime? date = null;
            date = user.As<UserPart>().LastPasswordChangeUtc;                       
            if (date == null) {
                date = user.As<UserPart>().CreatedUtc;
            }
            if (date != null) {
                passwordIsExpired = date.Value.AddDays(days) < _clock.UtcNow;
            }
            var securityPart = user.As<UserSecurityConfigurationPart>();
            var preventExpiration = securityPart != null && securityPart.PreventPasswordExpiration;
            return passwordIsExpired
                && !preventExpiration;
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

        private static void SetPasswordClear(UserPart userPart, string password) {
            userPart.PasswordFormat = MembershipPasswordFormat.Clear;
            userPart.Password = password;
            userPart.PasswordSalt = null;
        }

        private void SetPasswordHashed(UserPart userPart, string password) {
            var saltBytes = new byte[0x10];
            using (var random = new RNGCryptoServiceProvider()) {
                random.GetBytes(saltBytes);
            }

            userPart.PasswordFormat = MembershipPasswordFormat.Hashed;
            userPart.Password = PasswordExtensions.ComputeHashBase64(userPart.HashAlgorithm, saltBytes, password);
            userPart.PasswordSalt = Convert.ToBase64String(saltBytes);
        }

        private void SetPasswordEncrypted(UserPart userPart, string password) {
            userPart.Password = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(password)));
            userPart.PasswordSalt = null;
            userPart.PasswordFormat = MembershipPasswordFormat.Encrypted;
        }

    }
}
