using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Configuration;

namespace Orchard.Core.Common {
    public class Migrations : DataMigrationImpl {
        private readonly IRepository<IdentityPartRecord> _identityPartRepository;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly ShellSettings _shellSettings;

        private IList<string> existingIndexNames;


        public Migrations(
            IRepository<IdentityPartRecord> identityPartRepository,
            ISessionFactoryHolder sessionFactoryHolder,
            ShellSettings shellSettings) {
            _identityPartRepository = identityPartRepository;
            _sessionFactoryHolder = sessionFactoryHolder;
            _shellSettings = shellSettings;
        }


        public int Create() {
            SchemaBuilder.CreateTable("BodyPartRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("Text", column => column.Unlimited())
                .Column<string>("Format"));

            SchemaBuilder
                .CreateTable("CommonPartRecord", table => table
                    .ContentPartRecord()
                    .Column<int>("OwnerId")
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("PublishedUtc")
                    .Column<DateTime>("ModifiedUtc")
                    .Column<int>("Container_id"))
                .AlterTable(nameof(CommonPartRecord), table => {
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_{nameof(CommonPartRecord.CreatedUtc)}", nameof(CommonPartRecord.CreatedUtc));
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_{nameof(CommonPartRecord.ModifiedUtc)}", nameof(CommonPartRecord.ModifiedUtc));
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_{nameof(CommonPartRecord.PublishedUtc)}", nameof(CommonPartRecord.PublishedUtc));
                    // This originally in UpdateFrom7
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_Container_id", "Container_id");
                    // This originally in UpdateFrom8
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_OwnedBy_ByCreation",
                        nameof(CommonPartRecord.OwnerId),
                        nameof(CommonPartRecord.CreatedUtc));
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_OwnedBy_ByModification",
                        nameof(CommonPartRecord.OwnerId),
                        nameof(CommonPartRecord.ModifiedUtc));
                    table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_OwnedBy_ByPublication",
                        nameof(CommonPartRecord.OwnerId),
                        nameof(CommonPartRecord.PublishedUtc));
                });

            SchemaBuilder
                .CreateTable("CommonPartVersionRecord", table => table
                    .ContentPartVersionRecord()
                    .Column<DateTime>("CreatedUtc")
                    .Column<DateTime>("PublishedUtc")
                    .Column<DateTime>("ModifiedUtc")
                    .Column<string>("ModifiedBy"))
                .AlterTable(nameof(CommonPartVersionRecord), table => {
                    table.CreateIndex($"IDX_{nameof(CommonPartVersionRecord)}_{nameof(CommonPartVersionRecord.CreatedUtc)}", nameof(CommonPartVersionRecord.CreatedUtc));
                    table.CreateIndex($"IDX_{nameof(CommonPartVersionRecord)}_{nameof(CommonPartVersionRecord.ModifiedUtc)}", nameof(CommonPartVersionRecord.ModifiedUtc));
                    table.CreateIndex($"IDX_{nameof(CommonPartVersionRecord)}_{nameof(CommonPartVersionRecord.PublishedUtc)}", nameof(CommonPartVersionRecord.PublishedUtc));
                });

            SchemaBuilder
                .CreateTable(nameof(IdentityPartRecord), table => table
                    .ContentPartRecord()
                    .Column<string>("Identifier", column => column.WithLength(255)))
                .AlterTable(nameof(IdentityPartRecord), table => table
                    // This originally in UpdateFrom6
                    .CreateIndex($"IDX_{nameof(IdentityPartRecord)}_{nameof(IdentityPartRecord.Identifier)}", nameof(IdentityPartRecord.Identifier)));

            ContentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder
                .Attachable()
                .WithDescription("Allows the editing of text using an editor provided by the configured flavor (e.g. html, text, markdown)."));

            ContentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder
                .Attachable()
                .WithDescription("Provides common information about a content item, such as Owner, Date Created, Date Published and Date Modified."));

            ContentDefinitionManager.AlterPartDefinition("IdentityPart", builder => builder
                .Attachable()
                .WithDescription("Automatically generates a unique identity for the content item, which is required in import/export scenarios where one content item references another."));

            return 9;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("IdentityPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Identifier", column => column.Unlimited()));

            ContentDefinitionManager.AlterPartDefinition("IdentityPart", builder => builder.Attachable());

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("BodyPart", builder => builder
                .WithDescription("Allows the editing of text using an editor provided by the configured flavor (e.g. html, text, markdown)."));

            ContentDefinitionManager.AlterPartDefinition("CommonPart", builder => builder
                .WithDescription("Provides common information about a content item, such as Owner, Date Created, Date Published and Date Modified."));

            ContentDefinitionManager.AlterPartDefinition("IdentityPart", builder => builder
                .WithDescription("Automatically generates a unique identity for the content item, which is required in import/export scenarios where one content item references another."));

            return 3;
        }

        public int UpdateFrom3() {
            var existingIdentityParts = _identityPartRepository.Table.ToArray();

            foreach (var existingIdentityPart in existingIdentityParts) {
                if (existingIdentityPart.Identifier.Length > 255) {
                    throw new ArgumentException("Identifier '" + existingIdentityPart + "' is over 255 characters");
                }
            }

            SchemaBuilder.AlterTable("IdentityPartRecord", table => table
                .DropColumn("Identifier"));
            SchemaBuilder.AlterTable("IdentityPartRecord", table => table
                .AddColumn<string>("Identifier", command => command.WithLength(255)));

            foreach (var existingIdentityPart in existingIdentityParts) {
                var updateIdentityPartRecord = _identityPartRepository.Get(existingIdentityPart.Id);

                updateIdentityPartRecord.Identifier = existingIdentityPart.Identifier;

                _identityPartRepository.Update(updateIdentityPartRecord);
            }

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("CommonPartVersionRecord", table => table
                .AddColumn<string>("ModifiedBy", command => command.Nullable()));

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable(nameof(CommonPartRecord), table => {
                table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_{nameof(CommonPartRecord.CreatedUtc)}", nameof(CommonPartRecord.CreatedUtc));
                table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_{nameof(CommonPartRecord.ModifiedUtc)}", nameof(CommonPartRecord.ModifiedUtc));
                table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_{nameof(CommonPartRecord.PublishedUtc)}", nameof(CommonPartRecord.PublishedUtc));
            });

            SchemaBuilder.AlterTable(nameof(CommonPartVersionRecord), table => {
                table.CreateIndex($"IDX_{nameof(CommonPartVersionRecord)}_{nameof(CommonPartVersionRecord.CreatedUtc)}", nameof(CommonPartVersionRecord.CreatedUtc));
                table.CreateIndex($"IDX_{nameof(CommonPartVersionRecord)}_{nameof(CommonPartVersionRecord.ModifiedUtc)}", nameof(CommonPartVersionRecord.ModifiedUtc));
                table.CreateIndex($"IDX_{nameof(CommonPartVersionRecord)}_{nameof(CommonPartVersionRecord.PublishedUtc)}", nameof(CommonPartVersionRecord.PublishedUtc));
            });

            return 6;
        }

        // When upgrading from version 6 of 1.10.x, we'll just execute the same steps, but in a different order.
        public int UpdateFrom6() {
            // This is the original step of the dev branch.
            AddIndexForIdentityPartRecordIdentifier();

            return 7;
        }

        public int UpdateFrom7() {
            // This is the original step of the dev branch.
            AddIndexForCommonPartRecordContainerId();

            // When upgrading from version 7 of 1.10.x, this index isn't created yet, so we need to run this step
            // "again". On the other hand, AddIndexesForCommonPartOwner in UpdateFrom8 won't do anything, because those
            // indexes were added in the 1.10.x version of UpdateFrom6.
            AddIndexForIdentityPartRecordIdentifier();

            return 8;
        }

        public int UpdateFrom8() {
            // This is the original step of the dev branch.
            AddIndexesForCommonPartOwner();

            // When upgrading from version 8 of 1.10.x, this index isn't created yet, so we need to run this step
            // "again"
            AddIndexForCommonPartRecordContainerId();

            return 9;
        }

        // This change was originally UpdateFrom7 on 1.10.x and UpdateFrom6 on dev.
        private void AddIndexForIdentityPartRecordIdentifier() {
            var indexName = $"IDX_{nameof(IdentityPartRecord)}_{nameof(IdentityPartRecord.Identifier)}";

            if (IndexExists(nameof(IdentityPartRecord), indexName)) return;

            SchemaBuilder.AlterTable(nameof(IdentityPartRecord), table => table.CreateIndex(
                indexName,
                nameof(IdentityPartRecord.Identifier)));
        }

        // This change was originally UpdateFrom8 on 1.10.x and UpdateFrom7 on dev.
        private void AddIndexForCommonPartRecordContainerId() {
            var indexName = $"IDX_{nameof(CommonPartRecord)}_Container_id";

            if (IndexExists(nameof(CommonPartRecord), indexName)) return;

            // Container_Id is used in several queries like a foreign key.
            SchemaBuilder.AlterTable(nameof(CommonPartRecord), table => table.CreateIndex(indexName, "Container_id"));
        }

        // This change was originally UpdateFrom6 on 1.10.x and UpdateFrom8 on dev.
        private void AddIndexesForCommonPartOwner() {
            // Studying SQL Server query execution plans we noticed that when the system
            // tries to find content items for requests such as
            // "The items of type TTT owned by me, ordered from the most recent"
            // the existing indexes are not used. SQL Server does an index scan on the
            // Primary key for CommonPartRecord. This may lead to annoying deadlocks when
            // there are two concurrent transactions that are doing both this kind of query
            // as well as an update (or insert) in the CommonPartRecord.
            // Tests show that this can be easily fixed by adding a non-clustered index
            // with these keys: OwnerId, {one of PublishedUTC, ModifiedUTC, CreatedUTC}.
            // That means we need three indexes (one for each DateTime) to support ordering
            // on either of them.

            // The queries we analyzed look like (in pseudo sql)
            // SELECT TOP (N) *
            // FROM
            //   ContentItemVersionRecord this_
            //   inner join ContentItemRecord contentite1_ on this_.ContentItemRecord_id=contentite1_.Id
            //   inner join CommonPartRecord commonpart2_ on contentite1_.Id=commonpart2.Id
            //   left outer join ContentTypeRecord contenttyp6_ on contentite1_.ContentType_id=contenttyp6_.Id
            // WHERE
            //   contentite1.ContentType_id = {TTT}
            //   and commonpart2_.OwnerId = {userid}
            //   and this_.Published = 1
            // ORDER BY
            //   commonpart2_PublishedUtc desc
            var createdUtcIndexName = $"IDX_{nameof(CommonPartRecord)}_OwnedBy_ByCreation";

            if (IndexExists(nameof(CommonPartRecord), createdUtcIndexName)) return;

            SchemaBuilder.AlterTable(nameof(CommonPartRecord), table => {
                table.CreateIndex(createdUtcIndexName,
                    nameof(CommonPartRecord.OwnerId),
                    nameof(CommonPartRecord.CreatedUtc));
                table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_OwnedBy_ByModification",
                    nameof(CommonPartRecord.OwnerId),
                    nameof(CommonPartRecord.ModifiedUtc));
                table.CreateIndex($"IDX_{nameof(CommonPartRecord)}_OwnedBy_ByPublication",
                    nameof(CommonPartRecord.OwnerId),
                    nameof(CommonPartRecord.PublishedUtc));
            });
        }

        private bool IndexExists(string tableName, string indexName) {
            if (existingIndexNames == null) {
                // Database-agnostic way of checking the existence of an index.
                using (var session = _sessionFactoryHolder.GetSessionFactory().OpenSession()) {
                    var connection = session.Connection;

                    if (connection == null) {
                        throw new InvalidOperationException("The database connection object should derive from DbConnection to check if a table exists.");
                    }

                    existingIndexNames = connection.GetSchema("Indexes").Rows.Cast<DataRow>().Select(row =>
                        $"{row["TABLE_NAME"]}.{row["INDEX_NAME"]}").ToList();
                }
            }

            var indexNamePrefix = string.IsNullOrEmpty(_shellSettings.DataTablePrefix)
                ? string.Empty : $"{_shellSettings.DataTablePrefix}_";
            return existingIndexNames.Contains($"{SchemaBuilder.TableDbName(tableName)}.{indexNamePrefix}{indexName}");
        }
    }
}