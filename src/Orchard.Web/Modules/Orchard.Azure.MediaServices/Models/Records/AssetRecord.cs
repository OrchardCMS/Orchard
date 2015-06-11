using System;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Data.Conventions;

namespace Orchard.Azure.MediaServices.Models.Records {
    public class AssetRecord {

        public AssetRecord() {
            Infoset = new Infoset();
        }

        public virtual int Id { get; set; }
        public virtual int VideoContentItemId { get; set; }
        public virtual string Type { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string WamsPublicLocatorId { get; set; }
        public virtual string WamsPublicLocatorUrl { get; set; }
        public virtual string WamsPrivateLocatorId { get; set; }
        public virtual string WamsPrivateLocatorUrl { get; set; }
        public virtual string WamsAssetId { get; set; }
        public virtual string WamsEncoderMetadataXml { get; set; }
        public virtual string OriginalFileName { get; set; }
        public virtual string LocalTempFileName { get; set; }
        public virtual long? LocalTempFileSize { get; set; }
        public virtual bool IncludeInPlayer { get; set; }
        public virtual string MediaQuery { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual AssetUploadStatus UploadStatus { get; set; }
        public virtual DateTime? UploadStartedUtc { get; set; }
        public virtual DateTime? UploadCompletedUtc { get; set; }
        public virtual long? UploadBytesComplete { get; set; }
        public virtual AssetPublishStatus PublishStatus { get; set; }
        public virtual DateTime? PublishedUtc { get; set; }
        public virtual DateTime? RemovedUtc { get; set; }

        [StringLengthMax]
        public virtual string Data {
            get { return Infoset.Data; }
            set { Infoset.Data = value; }
        }

        public virtual Infoset Infoset { get; protected set; }
    }
}
