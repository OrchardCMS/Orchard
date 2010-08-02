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
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    [UsedImplicitly]
    public class MembershipService : IMembershipService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<UserPartRecord> _userRepository;

        public MembershipService(IContentManager contentManager, IRepository<UserPartRecord> userRepository) {
            _contentManager = contentManager;
            _userRepository = userRepository;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public MembershipSettings GetSettings() {
            var settings = new MembershipSettings();
            // accepting defaults
            return settings;
        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            Logger.Information("CreateUser {0} {1}", createUserParams.Username, createUserParams.Email);

            return _contentManager.Create<UserPart>(UserPartDriver.ContentType.Name, init =>
            {
                init.Record.UserName = createUserParams.Username;
                init.Record.Email = createUserParams.Email;
                init.Record.NormalizedUserName = createUserParams.Username.ToLower();
                init.Record.HashAlgorithm = "SHA1";
                SetPassword(init.Record, createUserParams.Password);
            });
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
