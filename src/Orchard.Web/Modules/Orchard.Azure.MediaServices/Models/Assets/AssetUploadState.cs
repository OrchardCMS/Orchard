using System;
using Orchard.Azure.MediaServices.Models.Records;

namespace Orchard.Azure.MediaServices.Models.Assets {
	public class AssetUploadState {

		private readonly AssetRecord _record;

		public AssetUploadState(AssetRecord record) {
			_record = record;
		}

		public AssetUploadStatus Status {
			get {
				return _record.UploadStatus;
			}
			set {
				_record.UploadStatus = value;
			}
		}

		public DateTime? StartedUtc {
			get {
				return _record.UploadStartedUtc;
			}
			set {
				_record.UploadStartedUtc = value;
			}
		}

		public DateTime? CompletedUtc {
			get {
				return _record.UploadCompletedUtc;
			}
			set {
				_record.UploadCompletedUtc = value;
			}
		}

		public long? BytesComplete {
			get {
				return _record.UploadBytesComplete;
			}
			set {
				_record.UploadBytesComplete = value;
			}
		}

		public double? PercentComplete {
			get {
				return BytesComplete.HasValue ? (double?)BytesComplete.Value / (double?)_record.LocalTempFileSize * 100 : (double?)null;
			}
		}
	}
}