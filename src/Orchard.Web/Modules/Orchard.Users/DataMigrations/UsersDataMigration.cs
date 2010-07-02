using Orchard.Data.Migration;

namespace Orchard.Users.DataMigrations {
    public class UsersDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Users_UserRecord (Id INTEGER not null, UserName TEXT, Email TEXT, NormalizedUserName TEXT, Password TEXT, PasswordFormat TEXT, PasswordSalt TEXT, primary key (Id));
            SchemaBuilder.CreateTable("UserRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("UserName")
                .Column<string>("Email")
                .Column<string>("NormalizedUserName")
                .Column<string>("Password")
                .Column<string>("PasswordFormat")
                .Column<string>("PasswordSalt")
                );

            return 0010;
        }
    }
}