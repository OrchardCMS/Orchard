using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using Orchard.Azure.Authentication.Models;

namespace Orchard.Azure.Authentication.Drivers {
    public class AzureActiveDirectorySyncPartDriver : ContentPartDriver<AzureActiveDirectorySyncPart> {
        protected override DriverResult Display(AzureActiveDirectorySyncPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_AzureActiveDirectorySync",
                () => shapeHelper.Parts_AzureActiveDirectorySync(
                    UserId: part.UserId,
                    LastSyncedUtc: part.LastSyncedUtc));
        }

        protected override DriverResult Editor(AzureActiveDirectorySyncPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AzureActiveDirectorySync_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts.AzureActiveDirectorySync",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(AzureActiveDirectorySyncPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(AzureActiveDirectorySyncPart part, ExportContentContext context) {
            ExportInfoset(part, context);

            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("UserId", part.UserId);
            element.SetAttributeValue("LastSyncedUtc", part.LastSyncedUtc);
        }

        protected override void Importing(AzureActiveDirectorySyncPart part, ImportContentContext context) {
            ImportInfoset(part, context);

            var partName = part.PartDefinition.Name;

            context.ImportAttribute(partName, "UserId", value => part.UserId = int.Parse(value));
            context.ImportAttribute(partName, "LastSyncedUtc", value => part.LastSyncedUtc = DateTime.Parse(value));
            // the incoming data is UTC.  Ensure we have the correct Offset
            if (null != part.LastSyncedUtc && ((DateTime) part.LastSyncedUtc).Hour != ((DateTime) part.LastSyncedUtc).ToUniversalTime().Hour) {
                DateTime dateTime = (DateTime) part.LastSyncedUtc;
                part.LastSyncedUtc = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
            }
        }
    }
}