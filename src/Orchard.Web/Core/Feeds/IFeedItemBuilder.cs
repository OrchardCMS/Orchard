using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds {
    public interface IFeedItemBuilder : IEvents {
        void Populate(FeedContext context);
    }
}
