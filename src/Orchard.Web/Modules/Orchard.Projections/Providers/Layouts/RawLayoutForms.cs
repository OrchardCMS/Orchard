using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Layouts {

    public class RawLayoutForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public RawLayoutForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "RawLayout",
                        _HtmlProperties: Shape.Fieldset(
                            Title: T("Html properties"),
                            _ContainerTag: Shape.TextBox(
                                Id: "container-tag", Name: "ContainerTag",
                                Title: T("Container tag"),
                                Description: T("The tag of the container. Leave empty for no container."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _ContainerId: Shape.TextBox(
                                Id: "container-id", Name: "ContainerId",
                                Title: T("Container id"),
                                Description: T("The id to provide on the container element."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _ContainerClass: Shape.TextBox(
                                Id: "container-class", Name: "ContainerClass",
                                Title: T("Container class"),
                                Description: T("The class to provide on the container element."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _ItemTag: Shape.TextBox(
                                Id: "item-tag", Name: "ItemTag",
                                Title: T("Item tag"),
                                Description: T("The tag of each item. Leave empty for no tag."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _ItemClass: Shape.TextBox(
                                Id: "item-class", Name: "ItemClass",
                                Title: T("Item class"),
                                Description: T("The class to provide on each item."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _Prepend: Shape.TextBox(
                                Id: "prepend", Name: "Prepend",
                                Title: T("Prepend"),
                                Description: T("Some HTML to insert before the first element."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _Separator: Shape.TextBox(
                                Id: "separator", Name: "Separator",
                                Title: T("Separator"),
                                Description: T("Some HTML to insert between two items."),
                                Classes: new[] { "text medium", "tokenized" }
                                ),
                            _Append: Shape.TextBox(
                                Id: "append", Name: "Append",
                                Title: T("Append"),
                                Description: T("Some HTML to insert after the last element."),
                                Classes: new[] { "text medium", "tokenized" }
                                )
                            )
                        );

                    return f;
                };

            context.Form("RawLayout", form);

        }
    }
}