using System;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Tags.Services;

namespace Orchard.Tags.Projections {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

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
                            ),
                        _Exclusion: Shape.FieldSet(
                            _OperatorOneOf: Shape.Radio(
                                Id: "operator-is-one-of", Name: "Operator",
                                Title: T("Is one of"), Value: "0", Checked: true
                                ),
                            _OperatorIsAllOf: Shape.Radio(
                                Id: "operator-is-all-of", Name: "Operator",
                                Title: T("Is all of"), Value: "1"
                                )
                            ));

                    foreach (var tag in _tagService.GetTags()) {
                        f._Tags.Add(new SelectListItem { Value = tag.Id.ToString(), Text = tag.TagName });
                    }

                    return f;
                };

            context.Form("SelectTags", form);

        }
    }
}