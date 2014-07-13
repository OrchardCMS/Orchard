using System;
using Orchard.Azure.MediaServices.Models.Records;

namespace Orchard.Azure.MediaServices.Models.Assets {
	public class AssetPublishState {

		private readonly AssetRecord _record;

		public AssetPublishState(AssetRecord record) {
			_record = record;
		}

		public AssetPublishStatus Status {
			get {
				return _record.PublishStatus;
			}
			set {
				_record.PublishStatus = value;
			}
		}

		public DateTime? PublishedUtc {
			get {
				return _record.PublishedUtc;
			}
			set {
				_record.PublishedUtc = value;
			}
		}

		public DateTime? RemovedUtc {
			get {
				return _record.RemovedUtc;
			}
			set {
				_record.RemovedUtc = value;
			}
		}
	}
}