using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;

namespace Orchard.ImportExport.Models {
    [OrchardFeature("Orchard.Deployment")]
    public class RemoteOrchardDeploymentPartRecord : ContentPartRecord {
        public virtual string BaseUrl { get; set; }
        public virtual string UserName { get; set; }
        public virtual string PrivateApiKey { get; set; }
    }

    [OrchardFeature("Orchard.Deployment")]
    public class RemoteOrchardDeploymentPart : ContentPart<RemoteOrchardDeploymentPartRecord> {
        private readonly ComputedField<string> _privateApiKey = new ComputedField<string>();

        public ComputedField<string> PrivateApiKeyField
        {
            get { return _privateApiKey; }
        }

        public string BaseUrl {
            get { return Record.BaseUrl; }
            set { Record.BaseUrl = value; }
        }

        public string UserName {
            get { return Record.UserName; }
            set { Record.UserName = value; }
        }

        public string PrivateApiKey {
            get { return _privateApiKey.Value; }
            set { _privateApiKey.Value = value; }
        }
    }
}