using System;
using Orchard.Core.Common.Models;
using Orchard.Security;

namespace Orchard.Core.Common.ViewModels {
    public class CommonMetadataViewModel {
        private readonly CommonAspect _commonAspect;

        public CommonMetadataViewModel(CommonAspect commonAspect) {
            _commonAspect = commonAspect;
        }

        public IUser Creator { get { return _commonAspect.Owner; } }

        public DateTime? CreatedUtc { get { return _commonAspect.CreatedUtc; } }
        public DateTime? PublishedUtc { get { return _commonAspect.PublishedUtc; } }
        public DateTime? ModifiedUtc { get { return _commonAspect.ModifiedUtc; } }

        public DateTime? VersionCreatedUtc { get { return _commonAspect.VersionCreatedUtc; } }
        public DateTime? VersionPublishedUtc { get { return _commonAspect.VersionPublishedUtc; } }
        public DateTime? VersionModifiedUtc { get { return _commonAspect.VersionModifiedUtc; } }
    }
}