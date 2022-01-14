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
                    .Column<DateTime>("LastPasswordChangeUtc", c => c.WithDefault(new DateTime(1990, 1, 1)))
                    .Column<bool>("ForcePasswordChange"))
                .AlterTable("UserPartRecord", table => table
                    .CreateIndex("IDX_UserPartRecord_NormalizedUserName", "NormalizedUserName"))
                // users are most commonly searched by NormalizedUserName and or Email
                .AlterTable("UserPartRecord", table => table
                    .CreateIndex($"IDX_UserPartRecord_NameAndEmail", "NormalizedUserName", "Email"));

            //Password History Table    
            SchemaBuilder
                .CreateTable("PasswordHistoryRecord", table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<int>("UserPartRecord_Id")
                    .Column<string>("Password")
                    .Column<string>("PasswordFormat")
                    .Column<string>("HashAlgorithm")
                    .Column<string>("PasswordSalt")
                    .Column<DateTime>("LastPasswordChangeUtc", c => c.WithDefault(new DateTime(1990, 1, 1))))
                .AlterTable("PasswordHistoryRecord", table => table
                        .CreateIndex($"IDX_UserPartRecord_Id", "UserPartRecord_Id"));

            // Queryable bool to tell which users should not be suspended automatically
            SchemaBuilder
                .CreateTable("UserSecurityConfigurationPartRecord", table => table
                    .ContentPartRecord()
                    .Column<bool>("SaveFromSuspension")
                    .Column<bool>("PreventPasswordExpiration"));

            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg.Creatable(false));

            return 9;
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
        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("UserPartRecord", table => {
                table.AddColumn<bool>("ForcePasswordChange");
            });
            SchemaBuilder
                .CreateTable("PasswordHistoryRecord", table => table
                    .Column<int>("Id", col => col.PrimaryKey().Identity())
                    .Column<int>("UserPartRecord_Id")
                    .Column<string>("Password")
                    .Column<string>("PasswordFormat")
                    .Column<string>("HashAlgorithm")
                    .Column<string>("PasswordSalt")
                    .Column<DateTime>("LastPasswordChangeUtc"))
                .AlterTable("PasswordHistoryRecord", table => table
                    .CreateIndex($"IDX_UserPartRecord_Id", "UserPartRecord_Id"));
            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder
                .CreateTable("UserSecurityConfigurationPartRecord", table => table
                    .ContentPartRecord()
                    .Column<bool>("SaveFromSuspension")
                    .Column<bool>("PreventPasswordExpiration"));

            return 9;
        }
    }
}