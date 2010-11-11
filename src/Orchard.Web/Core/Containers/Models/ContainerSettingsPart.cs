using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Containers.Models {
    public class ContainerSettingsPart : ContentPart<ContainerSettingsPartRecord> {
    }

    public class ContainerSettingsPartRecord : ContentPartRecord {
        private int _defaultPageSize = 10;
        public virtual int DefaultPageSize {
            get { return _defaultPageSize; }
            set { _defaultPageSize = value; }
        }
    }
}
