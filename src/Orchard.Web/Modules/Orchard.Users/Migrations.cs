using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;

namespace Orchard.Users {
    public class UsersDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder
                .CreateTable("UserPartRecord", table => table
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
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("LastLoginUtc")
                    .Column<DateTime>("LastLogoutUtc")
                    .Column<DateTime>("LastPasswordChangeUtc", c => c.WithDefault(new DateTime(1990, 1, 1))))
                .AlterTable("UserPartRecord", table => table
                    .CreateIndex("IDX_UserPartRecord_NormalizedUserName", "NormalizedUserName"));

            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg.Creatable(false));

            return 6;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg.Creatable(false));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("UserPartRecord", table => {
                table.AddColumn<DateTime>("CreatedUtc");
                table.AddColumn<DateTime>("LastLoginUtc");
            });

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("UserPartRecord", table => {
                table.AddColumn<DateTime>("LastLogoutUtc");
            });

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("UserPartRecord", table => {
                table.AddColumn<DateTime>("LastPasswordChangeUtc", c => c.WithDefault(new DateTime(1990, 1, 1)));
            });

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("UserPartRecord", table => table
                .CreateIndex("IDX_UserPartRecord_NormalizedUserName", "NormalizedUserName"));

            return 6;
        }

        public int UpdateFrom6() {
            // users are most commonly searched by NormalizedUserName and or Email
            SchemaBuilder.AlterTable("UserPartRecord", table => {
                table.CreateIndex($"IDX_UserPartRecord_NameAndEmail",
                    "NormalizedUserName",
                    "Email");
            });
            return 7;
        }
    }
}