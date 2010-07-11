using System;
using Orchard.Core.Common.Utilities;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;

namespace Orchard.Core.Common.Models {
    public class CommonAspect : ContentPart<CommonRecord>, ICommonAspect {
        private readonly LazyField<IUser> _owner = new LazyField<IUser>();
        private readonly LazyField<IContent> _container = new LazyField<IContent>();
        private readonly LazyField<DateTime?> _scheduledPublishUtc = new LazyField<DateTime?>();

        public LazyField<IUser> OwnerField { get { return _owner; } }

        public LazyField<IContent> ContainerField { get { return _container; } }

        public LazyField<DateTime?> ScheduledPublishUtc { get { return _scheduledPublishUtc; } }

        public IUser Owner {
            get { return _owner.Value; }
            set { _owner.Value = value; }
        }

        public IContent Container {
            get { return _container.Value; }
            set { _container.Value = value; }
        }

        public DateTime? CreatedUtc {
            get { return Record.CreatedUtc; }
            set { Record.CreatedUtc = value; }
        }

        public DateTime? PublishedUtc {
            get { return Record.PublishedUtc; }
            set { Record.PublishedUtc = value; }
        }

        public DateTime? ModifiedUtc {
            get { return Record.ModifiedUtc; }
            set { Record.ModifiedUtc = value; }
        }

        CommonVersionRecord VersionRecord {
            get {
                var versionPart = this.As<ContentPart<CommonVersionRecord>>();
                return versionPart == null ? null : versionPart.Record;
            }
        }

        public DateTime? VersionCreatedUtc {
            get {
                return VersionRecord == null ? CreatedUtc : VersionRecord.CreatedUtc;
            }
            set {
                if (VersionRecord != null) {
                    VersionRecord.CreatedUtc = value;
                }
            }
        }

        public DateTime? VersionPublishedUtc {
            get {
                return VersionRecord == null ? PublishedUtc : VersionRecord.PublishedUtc;
            }
            set {
                if (VersionRecord != null) {
                    VersionRecord.PublishedUtc = value;
                }
            }
        }

        public DateTime? VersionModifiedUtc {
            get {
                return VersionRecord == null ? ModifiedUtc : VersionRecord.ModifiedUtc;
            }
            set {
                if (VersionRecord != null) {
                    VersionRecord.ModifiedUtc = value;
                }
            }
        }
    }
}
