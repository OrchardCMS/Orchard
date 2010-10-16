using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.PublishLater.Models;
using Orchard.Data.Migration;

namespace Orchard.Core.PublishLater {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("PublishLaterPart", builder => builder.Attachable());

            return 1;
        }
    }
}