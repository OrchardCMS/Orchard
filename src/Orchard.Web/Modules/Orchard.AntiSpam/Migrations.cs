using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.AntiSpam {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            
            ContentDefinitionManager.AlterPartDefinition("SubmissionLimitPart", cfg => cfg
                .Attachable()
                .WithDescription("Allows to filter content items based on submissions frequency.")
                );

            ContentDefinitionManager.AlterPartDefinition("ReCaptchaPart", cfg => cfg
                .Attachable()
                .WithDescription("Ensures content items are submitted by humans only.")
                );

            ContentDefinitionManager.AlterPartDefinition("SpamFilterPart", cfg => cfg
                .Attachable()
                .WithDescription("Allows to filter submitted content items based on spam filters.")
                );

            SchemaBuilder.CreateTable("SpamFilterPartRecord",
                table => table.ContentPartVersionRecord()
                    .Column<string>("Status", c => c.WithLength(64))
                );

            ContentDefinitionManager.AlterPartDefinition("JavaScriptAntiSpamPart",
                builder => builder
                    .Attachable()
                    .WithDescription("Prevents spambots to post the editor by requiring JavaScript support."));

            return 4;
        }

        public int UpdateFrom1() {

            SchemaBuilder.CreateTable("ReCaptchaSettingsPartRecord",
                table => table.ContentPartVersionRecord()
                    .Column<string>("PublicKey")
                    .Column<string>("PrivateKey")
                    .Column<bool>("TrustAuthenticatedUsers")
                );

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("SpamFilterPart", builder => builder
                .WithDescription("Allows to filter submitted content items based on spam filters."));

            ContentDefinitionManager.AlterPartDefinition("SubmissionLimitPart", builder => builder
                .WithDescription("Allows to filter content items based on submissions frequency."));

            ContentDefinitionManager.AlterPartDefinition("ReCaptchaPart", builder => builder
                .WithDescription("Ensures content items are submitted by humans only."));

            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("JavaScriptAntiSpamPart",
                builder => builder
                    .Attachable()
                    .WithDescription("Prevents spambots to post the editor by requiring JavaScript support."));

            return 4;
        }
    }


    [OrchardFeature("Akismet.Filter")]
    public class AkismetMigrations : DataMigrationImpl {

        public int Create() {
            return 1;
        }
    }


    [OrchardFeature("TypePad.Filter")]
    public class TypePadMigrations : DataMigrationImpl {

        public int Create() {
            return 1;
        }
    }
}