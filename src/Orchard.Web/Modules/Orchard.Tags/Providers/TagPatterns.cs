using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tags.Services;
using Orchard.Localization;
using Orchard.Tags.Models;
using System.Web.Routing;

namespace Orchard.Tags.Providers {
    public class TagPatterns {

        private readonly ITagService _tagService;

        public TagPatterns(
            ITagService tagService
            ) {
                _tagService = tagService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        
        public void Describe(dynamic describe) {
            describe.For<TagRecord>("Tags", T("Tags"), T("Tags url patterns"), (Func<TagRecord, string>)GetId, (Func<string, TagRecord>)GetTag)
                .Pattern("Tags", T("View all tags"), T("A list of all tags are displayed on this page"), (Func<TagRecord, RouteValueDictionary>)GetTagsRouteValues)
                .Pattern("View", T("View tagged content"), T("Tagged content will be listed on this Url for each tag"), (Func<TagRecord, RouteValueDictionary>)GetRouteValues);
        }

        protected RouteValueDictionary GetRouteValues(TagRecord tag) {
            return new RouteValueDictionary(new{
                area = "Orchard.Tags",
                controller = "Home",
                action = "Search",
                tagName = tag.TagName
            });
        }

        protected RouteValueDictionary GetTagsRouteValues(TagRecord tag) {
            return new RouteValueDictionary(new {
                area = "Orchard.Tags",
                controller = "Home",
                action = "Index"
            });
        }

        protected string GetId(TagRecord tag) {
            return tag.Id.ToString();
        }

        protected TagRecord GetTag(string id) {
            return _tagService.GetTag(Convert.ToInt32(id));
        }

        public void Suggest(dynamic suggest) {
            suggest.For("Tags")
                .Suggest("View", "tags/tag-name", "{Tag.Name.Slug}", T("Slugified tag name"))
                .Suggest("Tags", "tags", "tags", T("Plain /tags url"));
        }


    }
}