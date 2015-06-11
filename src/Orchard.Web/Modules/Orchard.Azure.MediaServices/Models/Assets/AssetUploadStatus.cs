using System;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Core.Common.Utilities;
using Orchard.Data.Conventions;

namespace Orchard.Azure.MediaServices.Models.Assets {
    public enum AssetUploadStatus {
        Pending,
        Uploading,
        Uploaded,
        Canceled
    }
}
