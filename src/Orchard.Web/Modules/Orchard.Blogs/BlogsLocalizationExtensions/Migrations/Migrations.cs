using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.Blogs.BlogsLocalizationExtensions.Migrations {
    [OrchardFeature("Orchard.Blogs.LocalizationExtensions")]
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("Blog",
        cfg => cfg
            .WithPart("LocalizationPart"));
            ContentDefinitionManager.AlterTypeDefinition("BlogPost",
        cfg => cfg
            .WithPart("LocalizationPart"));
            return 1;
        }
    }
}