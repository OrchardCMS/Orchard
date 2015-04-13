using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Handlers;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Migrations {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("DeploymentSubscriptionPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("IncludeMetadata")
                .Column<bool>("IncludeData")
                .Column<bool>("IncludeFiles")
                .Column<bool>("DeployAsDrafts")
                .Column<string>("VersionHistoryOption")
                .Column<string>("ContentTypes", col => col.Unlimited())
                .Column<string>("QueryIdentity")
                .Column<DateTime>("DeployedChangesToUtc", c => c.Nullable())
                .Column<string>("Filter")
                .Column<int>("DeploymentConfigurationId", c => c.Nullable())
                .Column<string>("DeploymentType"));

            SchemaBuilder.CreateTable("ScheduledTaskRunHistory", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("ContentItemRecord_id")
                .Column<string>("ExecutionId")
                .Column<DateTime>("RunStartUtc", c => c.Nullable())
                .Column<DateTime>("RunCompletedUtc", c => c.Nullable())
                .Column<string>("RunStatus"));

            SchemaBuilder.CreateTable("RecurringTaskPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("IsActive")
                .Column<int>("RepeatFrequencyInMinutes"));

            SchemaBuilder.CreateTable("RemoteOrchardDeploymentPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("BaseUrl")
                .Column<string>("UserName"));

            SchemaBuilder.CreateTable("DeployablePartRecord", table => table
                .ContentPartVersionRecord()
                .Column<DateTime>("ImportedPublishedUtc", c => c.Nullable())
                .Column<DateTime>("UnpublishedUtc", c => c.Nullable())
                .Column<bool>("Latest"));

            SchemaBuilder.CreateTable("DeployableItemTargetPartRecord", table => table
                .ContentPartVersionRecord()
                .Column<int>("DeployableContentId")
                .Column<int>("DeploymentTargetId")
                .Column<DateTime>("DeployedUtc", c => c.Nullable())
                .Column<string>("DeploymentStatus")
                .Column<string>("ExecutionId"));

            ContentDefinitionManager.AlterTypeDefinition("DeploymentSubscription", cfg => cfg
                .WithPart(typeof (DeploymentSubscriptionPart).Name)
                .WithPart(typeof (RecurringTaskPart).Name, part => part.WithSetting("TaskType", SubscriptionTaskHandler.TaskType))
                .WithPart(typeof (TitlePart).Name)
                .WithPart(typeof (CommonPart).Name)
                .WithPart(typeof (IdentityPart).Name));

            ContentDefinitionManager.AlterTypeDefinition("RemoteOrchardDeployment", cfg => cfg
                .WithPart(typeof (RemoteOrchardDeploymentPart).Name)
                .WithPart(typeof (TitlePart).Name)
                .WithPart(typeof (CommonPart).Name)
                .WithPart(typeof (IdentityPart).Name)
                .WithSetting("Stereotype", "DeploymentConfiguration"));

            ContentDefinitionManager.AlterTypeDefinition("DeployableItemTarget", cfg => cfg
                .WithPart(typeof (DeployableItemTargetPart).Name)
                .WithPart(typeof (CommonPart).Name));

            SchemaBuilder.CreateTable("DeploymentUserPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("PrivateApiKey", c => c.Unlimited()));

            SchemaBuilder.AlterTable("RemoteOrchardDeploymentPartRecord", table => table
                .AddColumn<string>("PrivateApiKey", c => c.Unlimited()));

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("DeploymentSubscriptionPartRecord", table => table
                .AddColumn<bool>("IncludeFiles"));
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("DeploymentSubscriptionPartRecord", table => table
                .AddColumn<bool>("DeployAsDrafts"));
            return 3;
        }
    }
}