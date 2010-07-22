using System;
using Orchard.Core.Common.Models;
using Orchard.Security;

namespace Orchard.Core.Common.ViewModels {
    public class CommonMetadataViewModel {
        private readonly CommonPart _commonPart;

        public CommonMetadataViewModel(CommonPart commonPart) {
            _commonPart = commonPart;
        }

        public IUser Creator { get { return _commonPart.Owner; } }

        public DateTime? CreatedUtc { get { return _commonPart.CreatedUtc; } }
        public DateTime? PublishedUtc { get { return _commonPart.PublishedUtc; } }
        public DateTime? ModifiedUtc { get { return _commonPart.ModifiedUtc; } }

        public DateTime? VersionCreatedUtc { get { return _commonPart.VersionCreatedUtc; } }
        public DateTime? VersionPublishedUtc { get { return _commonPart.VersionPublishedUtc; } }
        public DateTime? VersionModifiedUtc { get { return _commonPart.VersionModifiedUtc; } }
    }
}