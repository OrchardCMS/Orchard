using System.ComponentModel.DataAnnotations;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.UI.Models;

namespace Orchard.Wikis.Models {
    public class WikiSettings : ModelPartWithRecord<WikiSettingsRecord> {
    }

    public class WikiSettingsRecord : ModelPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }
        
        [Required]
        public virtual string WikiEditTheme { get; set; }
    }

    public class WikiSettingsDriver : ModelDriver {
        public WikiSettingsDriver(IRepository<WikiSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<WikiSettings>("site"));
            Filters.Add(new StorageFilterForRecord<WikiSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });

            //add to user... just for fun
            Filters.Add(new ActivatingFilter<WikiSettings>("user"));
        }

        protected override void GetEditors(GetModelEditorsContext context) {
            var model = context.Instance.As<WikiSettings>();
            if (model == null)
                return;

            context.Editors.Add(ModelTemplate.For(model.Record, "WikiSettings"));
        }

        protected override void UpdateEditors(UpdateModelContext context) {
            var model = context.Instance.As<WikiSettings>();
            if (model == null)
                return;

            context.Updater.TryUpdateModel(model.Record, "WikiSettings", null, null);
            context.Editors.Add(ModelTemplate.For(model.Record, "WikiSettings"));
        }
    }
}

