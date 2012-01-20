using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tokens;
using Orchard.Localization;
using Orchard.Tags.Models;

namespace Orchard.Tags.Providers {
    public class TagTokenProvider : ITokenProvider {

        public TagTokenProvider() {

            T = NullLocalizer.Instance;

        }
        public Localizer T { get; set; }
        public void Describe(DescribeContext context) {

            context.For("Tag", T("Tags"), T("Tag records"))
                .Token("Name", T("Tag name"), T("Tag name"), "Text");

        }

        public void Evaluate(EvaluateContext context) {
            context.For<TagRecord>("Tag")
                .Token("Name", t => t.TagName)
                // By chaining the name to text it can be slugified in Autoroute
                .Chain("Name", "Text", t => t.TagName);
        }
    }
}