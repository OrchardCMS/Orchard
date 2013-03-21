using System;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.ContentPicker.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Orchard.ContentPicker {
    public class Migrations : DataMigrationImpl {
        private readonly ISessionLocator _sessionLocator;
        private readonly ShellSettings _shellSettings;

        public Migrations(
            ISessionLocator sessionLocator, 
            ShellSettings shellSettings) {
            _sessionLocator = sessionLocator;
            _shellSettings = shellSettings;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public int Create() {

            SchemaBuilder.CreateTable("ContentMenuItemPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("ContentMenuItemRecord_id")
                );

            ContentDefinitionManager.AlterTypeDefinition("ContentMenuItem", cfg => cfg
                .WithPart("MenuPart")
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("ContentMenuItemPart")
                .DisplayedAs("Content Menu Item")
                .WithSetting("Description", "Adds a Content Item to the menu.")
                .WithSetting("Stereotype", "MenuItem")
                );

            ContentDefinitionManager.AlterPartDefinition("NavigationPart", builder => builder
                .Attachable()
                .WithDescription("Allows the management of Content Menu Items associated with a Content Item."));
            
            // copying records from previous version of ContentMenuItemPartRecord which was in Core
            var session = _sessionLocator.For(typeof (ContentItemRecord));
            
            var tablePrefix = String.IsNullOrEmpty(_shellSettings.DataTablePrefix) ? "" : _shellSettings.DataTablePrefix + "_";

            try {
                if (null != session.CreateSQLQuery("SELECT COUNT(*) FROM " + tablePrefix + "Navigation_ContentMenuItemPartRecord").UniqueResult()) {
                    // if no exception is thrown, we need to upgrade previous data
                    var records = session.CreateSQLQuery("SELECT * FROM " + tablePrefix + "Navigation_ContentMenuItemPartRecord").List();

                    foreach (dynamic record in records) {
                        try {

                            session.CreateSQLQuery("INSERT INTO " + tablePrefix + "Orchard_ContentPicker_ContentMenuItemPartRecord (Id, ContentMenuItemRecord_id) VALUES (:id, :cid)")
                                   .SetInt32("id", (int)record.Id)
                                   .SetInt32("cid", (int)record.ContentMenuItemRecord_id)
                                   .ExecuteUpdate();
                        }
                        catch (Exception e) {
                            Logger.Error(e, "Could not migrate ContentMenuItemRecord with Id: {0}", (int)record.Id);
                        }
                    }
                }

            }
            catch {
                // silently ignore exception as it means there is no content to migrate
            }

            return 1;
        }
    }
}