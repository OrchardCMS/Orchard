using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Tags.Models;
using Orchard.Tags.Services;

namespace Orchard.Tags.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class TagsFilter : IFilterProvider {
        private readonly ITagService _tagService;

        public TagsFilter(ITagService tagService) {
            _tagService = tagService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Tags", T("Tags"), T("Tags"))
                .Element("HasTags", T("Has Tags"), T("Tagged content items"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "SelectTags"
                );
        }

        public void ApplyFilter(dynamic context) {
            string tags = Convert.ToString(context.State.TagIds);
            if (!String.IsNullOrEmpty(tags)) {
                var ids = tags.Split(new[] { ',' }).Select(Int32.Parse).ToArray();
                var query = (IContentQuery<ContentItem>)context.Query;
                context.Query = query.Join<TagsPartRecord>().Where(x => x.Tags.Any(t => ids.Contains(t.TagRecord.Id)));
            }
        }

        public LocalizedString DisplayFilter(dynamic context) {
            string tags = Convert.ToString(context.State.TagIds);

            if (String.IsNullOrEmpty(tags)) {
                return T("Any tag");
            }

            var tagNames = tags.Split(new[] { ',' }).Select(x => _tagService.GetTag(Int32.Parse(x)).TagName);

            return T("Tagged with {0}", String.Join(", ", tagNames));
        }
    }
}