using Orchard.ContentManagement;
using Orchard.Data.Migration;
using Orchard.Email.Models;

namespace Orchard.Email {
    public class Migrations : DataMigrationImpl {
        private readonly IOrchardServices _orchardServices;

        public Migrations(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public int Create() {

            return 2;
        }

        public int UpdateFrom1() {

            var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
#pragma warning disable CS0618 // Type or member is obsolete
            // We need this here to migrate from EnableSsl to EncryptionMethod.
            smtpSettings.EncryptionMethod = smtpSettings.EnableSsl
                ? SmtpEncryptionMethod.SslTls
                : SmtpEncryptionMethod.None;
#pragma warning restore CS0618 // Type or member is obsolete

            return 2;
        }
    }
}