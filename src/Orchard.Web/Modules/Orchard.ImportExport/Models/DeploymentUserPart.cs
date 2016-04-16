using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;

namespace Orchard.ImportExport.Models {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentUserPartRecord : ContentPartRecord {
        public virtual string PrivateApiKey { get; set; }
    }

    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentUserPart : ContentPart<DeploymentUserPartRecord> {
        private readonly ComputedField<string> _privateApiKey = new ComputedField<string>();

        public ComputedField<string> PrivateApiKeyField {
            get { return _privateApiKey; }
        }

        public string PrivateApiKey {
            get { return _privateApiKey.Value; }
            set { _privateApiKey.Value = value; }
        }
    }
}