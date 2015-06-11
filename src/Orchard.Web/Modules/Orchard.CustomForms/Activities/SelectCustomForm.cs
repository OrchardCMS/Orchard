using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.CustomForms.Activities {
    public class SelectCustomForm : IFormProvider {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public SelectCustomForm(IShapeFactory shapeFactory, IContentManager contentManager) {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {

                    var f = Shape.Form(
                        Id: "AnyOfCustomForms",
                        _Parts: Shape.SelectList(
                            Id: "customforms", Name: "CustomForms",
                            Title: T("Custom Forms"),
                            Description: T("Select some custom forms."),
                            Size: 10,
                            Multiple: true
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    var query = _contentManager.Query().ForType("CustomForm", "CustomFormWidget");
                    var customForms = query.List().Select(x => new { ContentItem = x, Metadata = _contentManager.GetItemMetadata(x)});

                    foreach (var customForm in customForms.OrderBy(x => x.Metadata.DisplayText)) {
                        
                        f._Parts.Add(new SelectListItem { Value = customForm.Metadata.Identity.ToString(), Text = customForm.Metadata.DisplayText });
                    }

                    return f;
                };

            context.Form("SelectCustomForms", form);

        }
    }
}