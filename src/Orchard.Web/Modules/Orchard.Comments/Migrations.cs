using System;
using System.Collections.Generic;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Comments {
    public class Migrations : DataMigrationImpl {
    
        public int Create() {
            //CREATE TABLE Orchard_Comments_ClosedCommentsRecord (Id  integer, ContentItemId INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ClosedCommentsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("ContentItemId")
                );

            //CREATE TABLE Orchard_Comments_CommentPartRecord (Id INTEGER not null, Author TEXT, SiteName TEXT, UserName TEXT, Email TEXT, Status TEXT, CommentDateUtc DATETIME, CommentText TEXT, CommentedOn INTEGER, CommentedOnContainer INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("CommentPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Author")
                .Column<string>("SiteName")
                .Column<string>("UserName")
                .Column<string>("Email")
                .Column<string>("Status")
                .Column<DateTime>("CommentDateUtc")
                .Column<string>("CommentText", column => column.Unlimited())
                .Column<int>("CommentedOn")
                .Column<int>("CommentedOnContainer")
                );

            //CREATE TABLE Orchard_Comments_CommentSettingsPartRecord (Id INTEGER not null, ModerateComments INTEGER, EnableSpamProtection INTEGER, AkismetKey TEXT, AkismetUrl TEXT, primary key (Id));
            SchemaBuilder.CreateTable("CommentSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("ModerateComments")
                .Column<bool>("EnableSpamProtection")
                .Column<string>("AkismetKey")
                .Column<string>("AkismetUrl")
               );

            //CREATE TABLE Orchard_Comments_CommentsPartRecord (Id INTEGER not null, CommentsShown INTEGER, CommentsActive INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("CommentsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("CommentsShown")
                .Column<bool>("CommentsActive")
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Comment",
               cfg => cfg
                   .WithPart("CommentPart")
                   .WithPart("CommonPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("Blog",
               cfg => cfg
                   .WithPart("CommentsContainerPart")
                );

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition(typeof(CommentsPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Default", new ContentLocation { Zone = "primary", Position = "before.5" }},
                    {"Detail", new ContentLocation { Zone = "primary", Position = "after.5" }},
                    {"SummaryAdmin", new ContentLocation { Zone = "meta", Position = null }},
                    {"Summary", new ContentLocation { Zone = "meta", Position = "5" }},
                    {"Editor", new ContentLocation { Zone = "primary", Position = "10" }},
                }));

            ContentDefinitionManager.AlterPartDefinition(typeof(CommentsContainerPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"SummaryAdmin", new ContentLocation { Zone = "meta", Position = null }},
                    {"Summary", new ContentLocation { Zone = "meta", Position = null }},
                }));

            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("CommentsPart", builder => builder.Attachable());
            return 4;
        }
    }
}