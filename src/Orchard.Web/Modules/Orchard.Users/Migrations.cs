using Orchard.Data.Migration;

namespace Orchard.Users {
    public class UsersDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Users_UserRecord (Id INTEGER not null, UserName TEXT, Email TEXT, NormalizedUserName TEXT, Password TEXT, PasswordFormat TEXT, PasswordSalt TEXT, primary key (Id));
            SchemaBuilder.CreateTable("UserPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("UserName")
                .Column<string>("Email")
                .Column<string>("NormalizedUserName")
                .Column<string>("Password")
                .Column<string>("PasswordFormat")
                .Column<string>("HashAlgorithm")
                .Column<string>("PasswordSalt")
                );

            return 1;
        }

        public int UpdateFrom1() {

            // Adds registration fields to previous versions
            SchemaBuilder
                .AlterTable("UserPartRecord", table => table.AddColumn<string>("RegistrationStatus", c => c.WithDefault("Approved")))
                .AlterTable("UserPartRecord", table => table.AddColumn<string>("EmailStatus", c => c.WithDefault("Approved")))
                .AlterTable("UserPartRecord", table => table.AddColumn<string>("EmailChallengeToken"));

            // Site Settings record
            SchemaBuilder.CreateTable("RegistrationSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("UsersCanRegister", c => c.WithDefault(false))
                .Column<bool>("UsersMustValidateEmail", c => c.WithDefault(false))
                .Column<bool>("UsersAreModerated", c => c.WithDefault(false))
                .Column<bool>("NotifyModeration", c => c.WithDefault(false))
            );

            return 2;
        }
    }
}