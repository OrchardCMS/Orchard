using System;
using Orchard.Data.Migration;

namespace Orchard.Comments.DataMigrations {
    public class CommentsDataMigration : DataMigrationImpl {
    
        public int Create() {
            //CREATE TABLE Orchard_Comments_ClosedCommentsRecord (Id  integer, ContentItemId INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ClosedCommentsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("ContentItemId")
                );

            //CREATE TABLE Orchard_Comments_CommentRecord (Id INTEGER not null, Author TEXT, SiteName TEXT, UserName TEXT, Email TEXT, Status TEXT, CommentDateUtc DATETIME, CommentText TEXT, CommentedOn INTEGER, CommentedOnContainer INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("CommentRecord", table => table
                .ContentPartRecord()
                .Column<string>("Author")
                .Column<string>("SiteName")
                .Column<string>("UserName")
                .Column<string>("Email")
                .Column<string>("Status")
                .Column<DateTime>("CommentDateUtc")
                .Column<string>("CommentText", column => column.WithLength(10000))
                .Column<int>("CommentedOn")
                .Column<int>("CommentedOnContainer")
                );

            //CREATE TABLE Orchard_Comments_CommentSettingsRecord (Id INTEGER not null, ModerateComments INTEGER, EnableSpamProtection INTEGER, AkismetKey TEXT, AkismetUrl TEXT, primary key (Id));
            SchemaBuilder.CreateTable("CommentSettingsRecord", table => table
                .ContentPartRecord()
                .Column<bool>("ModerateComments")
                .Column<bool>("EnableSpamProtection")
                .Column<string>("AkismetKey")
                .Column<string>("AkismetUrl")
               );

            //CREATE TABLE Orchard_Comments_HasCommentsRecord (Id INTEGER not null, CommentsShown INTEGER, CommentsActive INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("HasCommentsRecord", table => table
                .ContentPartRecord()
                .Column<bool>("CommentsShown")
                .Column<bool>("CommentsActive")
                );

            return 0010;
        }
    }
}