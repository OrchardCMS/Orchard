using Orchard.Data.Migration;

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
                    .Column<bool>("UsersAreModerated", c => c.WithDefault(false))
                    .Column<bool>("NotifyModeration", c => c.WithDefault(false))
                    .Column<bool>("EnableLostPassword", c => c.WithDefault(false))
                );

            return 1;
        }
    }
}