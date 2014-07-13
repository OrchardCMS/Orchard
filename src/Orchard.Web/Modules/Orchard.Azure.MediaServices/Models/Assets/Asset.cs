using System;
using System.Collections;
using System.Collections.Generic;
using Orchard.Azure.MediaServices.Models.Records;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Core.Common.Utilities;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.MediaServices.Models.Assets {
    public abstract class Asset {

        internal readonly LazyField<CloudVideoPart> _videoPartField = new LazyField<CloudVideoPart>();
        private AssetRecord _record;

        public IFieldStorage Storage { get; set; }
        public IMimeTypeProvider MimeTypeProvider { get; set; }

        public AssetRecord Record {
            get { return _record; }
            internal set {
                _record = value;
                UploadState = new AssetUploadState(_record);
                PublishState = new AssetPublishState(_record);
            }
        }

        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        public bool IncludeInPlayer {
            get { return Record.IncludeInPlayer; }
            set { Record.IncludeInPlayer = value; }
        }

        public string MediaQuery {
            get { return Record.MediaQuery; }
            set { Record.MediaQuery = value; }
        }

        public DateTime CreatedUtc {
            get { return Record.CreatedUtc; }
            set { Record.CreatedUtc = value; }
        }

        public CloudVideoPart VideoPart {
            get { return _videoPartField.Value; }
            set { _videoPartField.Value = value; }
        }

        public string WamsPublicLocatorId {
            get { return Record.WamsPublicLocatorId; }
            set { Record.WamsPublicLocatorId = value; }
        }

        public string WamsPublicLocatorUrl {
            get { return Record.WamsPublicLocatorUrl; }
            set { Record.WamsPublicLocatorUrl = value; }
        }

        public string WamsPrivateLocatorId {
            get { return Record.WamsPrivateLocatorId; }
            set { Record.WamsPrivateLocatorId = value; }
        }

        public string WamsPrivateLocatorUrl {
            get { return Record.WamsPrivateLocatorUrl; }
            set { Record.WamsPrivateLocatorUrl = value; }
        }

        public string WamsAssetId {
            get { return Record.WamsAssetId; }
            set { Record.WamsAssetId = value; }
        }

        public string OriginalFileName {
            get { return Record.OriginalFileName; }
            set { Record.OriginalFileName = value; }
        }

        public string LocalTempFileName {
            get { return Record.LocalTempFileName; }
            set { Record.LocalTempFileName = value; }
        }

        public long? LocalTempFileSize {
            get { return Record.LocalTempFileSize; }
            set { Record.LocalTempFileSize = value; }
        }

        public string MimeType {
            get {
                var fileName = !String.IsNullOrWhiteSpace(PrivateMainFileUrl) ? GetFileName(PrivateMainFileUrl) : OriginalFileName;
                return MimeTypeProvider.GetMimeType(fileName);
            }
        }

        public string PrivateMainFileUrl {
            get { return GetMainFileUrl(WamsPrivateLocatorUrl); }
        }

        public string PublicMainFileUrl {
            get { return GetMainFileUrl(WamsPublicLocatorUrl); }
        }

        public AssetUploadState UploadState { get; private set; }
        public AssetPublishState PublishState { get; private set; }

        public virtual IEnumerable<DisplayLocator> GetDisplayLocators() {
            yield return new DisplayLocator("Private (SAS)", WamsPrivateLocatorId, WamsPrivateLocatorUrl);
            yield return new DisplayLocator("Public (SAS)", WamsPublicLocatorId, WamsPublicLocatorUrl);
        }

        public override string ToString() {
            return String.Format("{0} - {1}", GetType().Name, Name);
        }

        protected virtual string GetMainFileUrl(string locatorUrl) {
            if (!String.IsNullOrEmpty(locatorUrl)) {
                var builder = new UriBuilder(locatorUrl);
                builder.Path += "/" + OriginalFileName;
                return builder.Uri.AbsoluteUri;
            }
            return null;
        }

        private static string GetFileName(string fileUrl) {
            var uri = new Uri(fileUrl, UriKind.Absolute);
            return uri.AbsolutePath;
        }
    }
}