using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Data.Migration;
using Orchard.Users.Models;

namespace Orchard.Users {
    public class UsersDataMigration : DataMigrationImpl {

        public UsersDataMigration(IOrchardServices orchardServices) {
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }

        public int Create() {
            SchemaBuilder.CreateTable("UserPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("UserName")
                    .Column<string>("Email")
                    .Column<string>("NormalizedUserName", c => c.Unique())
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

            return 2;
        }

        public int UpdateFrom1() {
            IEnumerable<UserPart> users = Services.ContentManager.Query<UserPart, UserPartRecord>().List();

            foreach (UserPart user in users) {
                user.NormalizedUserName = user.UserName.ToUpperInvariant();
            }

            return 2;
        }
    }
}