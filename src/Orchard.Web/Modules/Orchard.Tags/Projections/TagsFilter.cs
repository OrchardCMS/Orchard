using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Tags.Models;
using Orchard.Tags.Services;

namespace Orchard.Tags.Projections {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    [OrchardFeature("Orchard.Tags.Projections")]
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

    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    [OrchardFeature("Orchard.Tags.Projections")]
    public class TagsFilterForms : IFormProvider {
        private readonly ITagService _tagService;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public TagsFilterForms(
            IShapeFactory shapeFactory,
            ITagService tagService) {
            _tagService = tagService;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context) {
            Func<IShapeFactory, dynamic> form =
                shape => {

                    var f = Shape.Form(
                        Id: "SelectTags",
                        _Tags: Shape.SelectList(
                            Id: "tagids", Name: "TagIds",
                            Title: T("Tags"),
                            Description: T("Select some tags."),
                            Size: 10,
                            Multiple: true
                            )
                        );

                    foreach (var tag in _tagService.GetTags()) {
                        f._Tags.Add(new SelectListItem { Value = tag.Id.ToString(), Text = tag.TagName });
                    }

                    return f;
                };

            context.Form("SelectTags", form);

        }
    }
}