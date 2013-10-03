using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Filters {
    public class ContentPartsForm : IFormProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ContentPartsForm(
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "AnyOfContentParts",
                        _Parts: Shape.SelectList(
                            Id: "contentparts", Name: "ContentParts",
                            Title: T("Content parts"),
                            Description: T("Select some content parts."),
                            Size: 10,
                            Multiple: true
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var contentPart in _contentDefinitionManager.ListPartDefinitions().OrderBy(x => x.Name)) {
                        f._Parts.Add(new SelectListItem { Value = contentPart.Name, Text = contentPart.Name });
                    }

                    return f;
                };

            context.Form("ContentPartsForm", form);

        }
    }
}