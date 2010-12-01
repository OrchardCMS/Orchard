using Orchard.ContentManagement.Records;

namespace Orchard.Users.Models {
    public class RegistrationSettingsPartRecord : ContentPartRecord {
        public virtual bool UsersCanRegister { get; set; }
        public virtual bool UsersMustValidateEmail { get; set; }
        public virtual bool UsersAreModerated { get; set; }
        public virtual bool NotifyModeration { get; set; }
        public virtual bool EnableLostPassword { get; set; }
    }
}