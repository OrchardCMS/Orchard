using Orchard.Core.Settings.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Settings.Models {
    public class SiteDriver : ModelDriverWithRecord<SiteSettingsRecord> {
        public SiteDriver(IRepository<SiteSettingsRecord> repository)
            : base(repository) {
        }

        protected override void New(NewModelContext context) {
            if (context.ModelType == "site") {
                context.Builder.Weld<SiteModel>();
            }
        }
    }
}
