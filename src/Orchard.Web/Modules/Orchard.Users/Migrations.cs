using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Core.Contents.Extensions;

namespace Orchard.Users {
    public class UsersDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("UserPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("UserName")
                    .Column<string>("Email")
                    .Column<string>("NormalizedUserName")
                    .Column<string>("Password")
                    .Column<string>("PasswordFormat")
                    .Column<string>("HashAlgorithm")
                    .Column<string>("PasswordSalt")
                    .Column<string>("RegistrationStatus", c => c.WithDefault("Approved"))
                    .Column<string>("EmailStatus", c => c.WithDefault("Approved"))
                    .Column<string>("EmailChallengeToken")
                );

            SchemaBuilder.CreateTable("RegistrationSettingsPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<bool>("UsersCanRegister", c => c.WithDefault(false))
                    .Column<bool>("UsersMustValidateEmail", c => c.WithDefault(false))
                    .Column<string>("ValidateEmailRegisteredWebsite", c => c.WithLength(255))
                    .Column<string>("ValidateEmailContactEMail", c => c.WithLength(255))
                    .Column<bool>("UsersAreModerated", c => c.WithDefault(false))
                    .Column<bool>("NotifyModeration", c => c.WithDefault(false))
                    .Column<string>("NotificationsRecipients", c => c.Unlimited())
                    .Column<bool>("EnableLostPassword", c => c.WithDefault(false))
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg.Creatable(false));

            return 2;
        }
    }
}