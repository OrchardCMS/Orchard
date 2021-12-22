using Orchard.ContentManagement.Records;

namespace Orchard.Users.Models {
    public class UserSecurityConfigurationPartRecord : ContentPartRecord {
        // We are creating a record for this rather than making do with the infoset
        // because this will allow us to explicitly query for those users that must
        // (or not) be saved from suspension.
        public virtual bool SaveFromSuspension { get; set; }
        public virtual bool PreventPasswordExpiration { get; set; }
    }
}