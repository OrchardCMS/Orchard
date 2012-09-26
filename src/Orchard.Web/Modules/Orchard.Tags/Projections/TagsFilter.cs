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

                if (ids.Length == 0) {
                    return;
                }

                int op = Convert.ToInt32(context.State.Operator);

                switch (op) {
                    case 0:
                        // is one of
                            Action<IAliasFactory> s = alias => alias.ContentPartRecord<TagsPartRecord>().Property("Tags", "tags").Property("TagRecord", "tagRecord");
                            Action<IHqlExpressionFactory> f = x => x.InG("Id", ids);
                            context.Query.Where(s, f);
                        break;
                    case 1:
                        // is all of
                        foreach (var id in ids) {
                            int tagId = id;
                            Action<IAliasFactory> selector = alias => alias.ContentPartRecord<TagsPartRecord>().Property("Tags", "tags" + tagId);
                            Action<IHqlExpressionFactory> filter = x => x.Eq("TagRecord.Id", tagId);
                            context.Query.Where(selector, filter);
                        }
                        break;
                    case 2:
                        // is not one of can't be done without sub queries
                        break;
                }
            }
        }

        public LocalizedString DisplayFilter(dynamic context) {
            string tags = Convert.ToString(context.State.TagIds);

            if (String.IsNullOrEmpty(tags)) {
                return T("Any tag");
            }

            var tagNames = tags.Split(new[] { ',' }).Select(x => _tagService.GetTag(Int32.Parse(x))).Where(x => x != null).Select(x => x.TagName);

            return T("Tagged with {0}", String.Join(", ", tagNames));
        }
    }
}