using System;
using System.Xml;
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
                var dateTime = Get("CreatedUtc");
                return String.IsNullOrEmpty(dateTime) ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                Set("CreatedUtc", dateTime);
                Record.CreatedUtc = value;
            }
        }

        public DateTime? PublishedUtc {
            get {
                var dateTime = Get("PublishedUtc");
                return String.IsNullOrEmpty(dateTime) ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                Set("PublishedUtc", dateTime);
                Record.PublishedUtc = value;
            }
        }

        public DateTime? ModifiedUtc {
            get {
                var dateTime = Get("ModifiedUtc");
                return String.IsNullOrEmpty(dateTime) ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                Set("ModifiedUtc", dateTime);
                Record.ModifiedUtc = value;
            }
        }

        public DateTime? VersionCreatedUtc {
            get {
                var dateTime = this.As<ContentPart<CommonPartVersionRecord>>().Get("CreatedUtc");
                return String.IsNullOrEmpty(dateTime) ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<ContentPart<CommonPartVersionRecord>>().Set("CreatedUtc", dateTime);
            }
        }

        public DateTime? VersionPublishedUtc {
            get {
                var dateTime = this.As<ContentPart<CommonPartVersionRecord>>().Get("PublishedUtc");
                return String.IsNullOrEmpty(dateTime) ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<ContentPart<CommonPartVersionRecord>>().Set("PublishedUtc", dateTime);
            }
        }

        public DateTime? VersionModifiedUtc {
            get {
                var dateTime = this.As<ContentPart<CommonPartVersionRecord>>().Get("ModifiedUtc");
                return String.IsNullOrEmpty(dateTime) ? (DateTime?)null : XmlConvert.ToDateTime(dateTime, XmlDateTimeSerializationMode.Utc);
            }
            set {
                string dateTime = value.HasValue ? XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.Utc) : "";
                this.As<ContentPart<CommonPartVersionRecord>>().Set("ModifiedUtc", dateTime);
            }
        }
    }
}
