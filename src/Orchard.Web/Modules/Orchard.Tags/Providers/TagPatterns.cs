using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tags.Services;
using Orchard.Localization;
using Orchard.Tags.Models;
using System.Web.Routing;
using Orchard.Events;

namespace Orchard.Tags.Providers {

    public interface IRoutePatternProvider : IEventHandler {
        void Describe(dynamic describe);
        void Suggest(dynamic suggest);
    }
    public class TagPatterns : IRoutePatternProvider {

        private readonly ITagService _tagService;

        public TagPatterns(
            ITagService tagService
            ) {
                _tagService = tagService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        
        public void Describe(dynamic describe) {
            describe.For<TagRecord>("Tags", T("Tags"), T("Tags url patterns"), (Func<TagRecord, string>)GetId, (Func<string, TagRecord>)GetTag, (Func<TagRecord, IDictionary<string, object>>)GetContext)
                .Pattern("Tags", T("View all tags"), T("A list of all tags are displayed on this page"), (Func<TagRecord, RouteValueDictionary>)GetTagsRouteValues)
                .Pattern("View", T("View tagged content"), T("Tagged content will be listed on this Url for each tag"), (Func<TagRecord, RouteValueDictionary>)GetRouteValues);
        }

        public RouteValueDictionary GetRouteValues(TagRecord tag) {
            return new RouteValueDictionary(new{
                area = "Orchard.Tags",
                controller = "Home",
                action = "Search",
                tagName = tag.TagName
            });
        }

        public RouteValueDictionary GetTagsRouteValues(TagRecord tag) {
            return new RouteValueDictionary(new {
                area = "Orchard.Tags",
                controller = "Home",
                action = "Index"
            });
        }

        public IDictionary<string,object> GetContext(TagRecord tag) {
            return new Dictionary<string, object> { { "Tag", tag } };
        }

        public string GetId(TagRecord tag) {
            return tag.Id.ToString();
        }

        public TagRecord GetTag(string id) {
            return _tagService.GetTag(Convert.ToInt32(id));
        }

        public void Suggest(dynamic suggest) {
            suggest.For("Tags")
                .Suggest("View", "tags/tag-name", "{Tag.Name.Slug}", T("Slugified tag name"))
                .Suggest("Tags", "tags", "tags", T("Plain /tags url"));
        }


    }
}