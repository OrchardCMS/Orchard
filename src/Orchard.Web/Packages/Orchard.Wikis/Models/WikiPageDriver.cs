using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Core.Common.Models;
using Orchard.Models.Driver;

namespace Orchard.Wikis.Models {
    public class WikiPageDriver : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "wikipage") {
                context.Builder
                    .Weld<CommonModel>()
                    .Weld<RoutableModel>()
                    .Weld<ContentModel>();
            }
        }
    }
}
