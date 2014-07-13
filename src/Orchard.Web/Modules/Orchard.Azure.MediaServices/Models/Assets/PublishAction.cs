using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;

namespace Orchard.Azure.MediaServices.Models.Assets {
    public enum PublishAction {
        None,
        Publish,
        PublishLater
    }
}
