using System;
using System.Xml;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Core.Common.Utilities;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;

namespace Orchard.Core.Common.Models {
    public class CommonPart : ContentPart<CommonPartRecord>, ICommonPart {
        private readonly LazyField<IUser> _owner = new LazyField<IUser>();
        private readonly LazyField<IContent> _container = new LazyField<IContent>();

        public LazyField<IUser> OwnerField { get { return _owner; } }

        public LazyField<IContent> ContainerField { get { return _container; } }

        public IUser Owner {
            get { return _owner.Value; }
            set { _owner.Value = value; }
        }

        public IContent Container {
            get { return _container.Value; }
            set { _container.Value = value; }
        }
        
        public DateTime? CreatedUtc {
            get {
                var dateTime = this.As<InfosetPart>().Get<CommonPart>("CreatedUtc");
                return dateTime == "" ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<InfosetPart>().Set<CommonPart>("CreatedUtc", dateTime);
                Record.CreatedUtc = value;
            }
        }

        public DateTime? PublishedUtc {
            get {
                var dateTime = this.As<InfosetPart>().Get<CommonPart>("PublishedUtc");
                return dateTime == "" ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<InfosetPart>().Set<CommonPart>("PublishedUtc", dateTime);
                Record.PublishedUtc = value;
            }
        }

        public DateTime? ModifiedUtc {
            get {
                var dateTime = this.As<InfosetPart>().Get<CommonPart>("ModifiedUtc");
                return dateTime == "" ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<InfosetPart>().Set<CommonPart>("ModifiedUtc", dateTime);
                Record.ModifiedUtc = value;
            }
        }

        CommonPartVersionRecord PartVersionRecord {
            get {
                var versionPart = this.As<ContentPart<CommonPartVersionRecord>>();
                return versionPart == null ? null : versionPart.Record;
            }
        }

        public DateTime? VersionCreatedUtc {
            get {
                var dateTime = this.As<InfosetPart>().Get<ContentPart<CommonPartVersionRecord>>("CreatedUtc");
                return dateTime == "" ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<InfosetPart>().Set<ContentPart<CommonPartVersionRecord>>("CreatedUtc", dateTime);
                if (PartVersionRecord != null)
                    PartVersionRecord.CreatedUtc = value;
            }
        }

        public DateTime? VersionPublishedUtc {
            get {
                var dateTime = this.As<InfosetPart>().Get<ContentPart<CommonPartVersionRecord>>("PublishedUtc");
                return dateTime == "" ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<InfosetPart>().Set<ContentPart<CommonPartVersionRecord>>("PublishedUtc", dateTime);
                if (PartVersionRecord != null)
                    PartVersionRecord.PublishedUtc = value;
            }
        }

        public DateTime? VersionModifiedUtc {
            get {
                var dateTime = this.As<InfosetPart>().Get<ContentPart<CommonPartVersionRecord>>("ModifiedUtc");
                return dateTime == "" ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<InfosetPart>().Set<ContentPart<CommonPartVersionRecord>>("ModifiedUtc", dateTime);
                if (PartVersionRecord != null)
                    PartVersionRecord.ModifiedUtc = value;
            }
        }

    }
}
