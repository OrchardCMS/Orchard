using System;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace Orchard.Azure.MediaServices.Services.Wams {
    public class WamsUploadProgressInfo {
        public WamsUploadProgressInfo(Guid assetMoniker, BlobTransferProgressChangedEventArgs data) {
            AssetMoniker = assetMoniker;
            Data = data;
        }
        public Guid AssetMoniker { get; private set; }
        public BlobTransferProgressChangedEventArgs Data { get; private set; }
    }
}