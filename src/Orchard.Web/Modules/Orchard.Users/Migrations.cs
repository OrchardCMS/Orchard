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

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg.Creatable(false));

            return 2;
        }
    }
}