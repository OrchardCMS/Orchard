using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Orchard.ImportExport.Models {
    [OrchardFeature("Orchard.Deployment")]
    public class DeployableItemTargetPartRecord : ContentPartVersionRecord {
        public virtual int DeployableContentId { get; set; }
        public virtual int DeploymentTargetId { get; set; }
        public virtual DateTime? DeployedUtc { get; set; }
        public virtual string DeploymentStatus { get; set; }
        public virtual string ExecutionId { get; set; }
    }

    public class DeployableItemTargetPart : ContentPart<DeployableItemTargetPartRecord> {
        private IContent _deployableContent;

        public IContent DeployableContent {
            get {
                _deployableContent = _deployableContent ?? ContentItem.ContentManager.Get(Record.DeployableContentId);
                return _deployableContent;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value", "DeployableContent cannot be set to null");
                }
                Record.DeployableContentId = value.Id;
                _deployableContent = value;
            }
        }

        public IContent DeploymentTarget {
            get {
                return ContentItem.ContentManager.Get<IContent>(Record.DeploymentTargetId);
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value", "DeploymentTarget cannot be set to null");
                }
                Record.DeploymentTargetId = value.Id;
            }
        }

        public DateTime? DeployedUtc {
            get { return Record.DeployedUtc; }
            set { Record.DeployedUtc = value; }
        }

        public DeploymentStatus DeploymentStatus {
            get {
                DeploymentStatus status;
                return Enum.TryParse(Record.DeploymentStatus, out status) ? status : DeploymentStatus.Unknown;
            }
            set { Record.DeploymentStatus = value.ToString(); }
        }

        public string ExecutionId
        {
            get { return Record.ExecutionId; }
            set { Record.ExecutionId = value; }
        }
    }

    public enum DeploymentStatus {
        Queued,
        Successful,
        Failed,
        Unknown
    }
}