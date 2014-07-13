using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Azure.MediaServices {
    public class Migrations : DataMigrationImpl {
        public int Create() {

            SchemaBuilder.CreateTable("AssetRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("VideoContentItemId")
                .Column<string>("Type")
                .Column<string>("Name", column => column.WithLength(256))
                .Column<string>("Description", column => column.Unlimited())
                .Column<string>("WamsPublicLocatorId")
                .Column<string>("WamsPublicLocatorUrl", column => column.WithLength(512))
                .Column<string>("WamsPrivateLocatorId")
                .Column<string>("WamsPrivateLocatorUrl", column => column.WithLength(512))
                .Column<string>("WamsAssetId", column => column.WithLength(64))
                .Column<string>("WamsEncoderMetadataXml", column => column.Unlimited())
                .Column<string>("OriginalFileName", column => column.WithLength(256))
                .Column<string>("LocalTempFileName", column => column.WithLength(64))
                .Column<long>("LocalTempFileSize")
                .Column<bool>("IncludeInPlayer")
                .Column<string>("MediaQuery", column => column.WithLength(256))
                .Column<DateTime>("CreatedUtc")
                .Column<string>("UploadStatus", column => column.WithLength(64))
                .Column<DateTime>("UploadStartedUtc")
                .Column<DateTime>("UploadCompletedUtc")
                .Column<long>("UploadBytesComplete")
                .Column<string>("PublishStatus", column => column.WithLength(64))
                .Column<DateTime>("PublishedUtc")
                .Column<DateTime>("RemovedUtc")
                .Column<string>("Data", column => column.Unlimited()));

            SchemaBuilder.CreateTable("JobRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("CloudVideoPartId")
                .Column<string>("WamsJobId", column => column.WithLength(64))
                .Column<string>("Name", column => column.WithLength(256))
                .Column<string>("Description", column => column.WithLength(1024))
                .Column<string>("Status", column => column.WithLength(32))
                .Column<string>("ErrorMessage", column => column.Unlimited())
                .Column<string>("OutputAssetName", column => column.WithLength(256))
                .Column<string>("OutputAssetDescription", column => column.Unlimited())
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("StartedUtc")
                .Column<DateTime>("FinishedUtc"));

            SchemaBuilder.CreateTable("TaskRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("JobId")
                .Column<string>("WamsTaskId", column => column.WithLength(64))
                .Column<string>("TaskProviderName", column => column.WithLength(64))
                .Column<int>("TaskIndex", column => column.NotNull())
                .Column<string>("Status", column => column.WithLength(32))
                .Column<int>("PercentComplete", column => column.NotNull())
                .Column<string>("SettingsXml", column => column.Unlimited())
                .Column<string>("HarvestAssetType", column => column.WithLength(64))
                .Column<string>("HarvestAssetName", column => column.WithLength(256)));

            ContentDefinitionManager.AlterPartDefinition("CloudVideoPart", part => part
                .Attachable(false)
                .WithDescription("Stores information about a cloud video item and its related assets and jobs in Microsoft Azure Media Services."));

            ContentDefinitionManager.AlterTypeDefinition("CloudVideo", type => type
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("MediaPart")
                .WithPart("TitlePart")
                .WithPart("PublishLaterPart")
                .WithPart("CloudVideoPart")
                .DisplayedAs("Cloud Video")
                .WithSetting("Stereotype", "Media")
                .Draftable());

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("AssetRecord", table => table
                   .AddColumn<int>("MediaQuery", column => column.WithLength(256)));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("AssetRecord", table => table
                .DropColumn("MimeType"));

            return 3;
        }
    }
}