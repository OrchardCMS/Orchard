using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.Projections.Providers.Filters {
    public class ContentTypesFilter : IFilterProvider {
        public ContentTypesFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Content", T("Content"),T("Content"))
                .Element("ContentTypes", T("Content Types"), T("Specific content types"),
                    ApplyFilter,
                    DisplayFilter,
                    "ContentTypesFilter"
                );

        }

        public void ApplyFilter(FilterContext context) {
            var contentTypes = (string)context.State.ContentTypes;
            if (!String.IsNullOrEmpty(contentTypes)) {
                context.Query = context.Query.ForType(contentTypes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            string contenttypes = context.State.ContentTypes;

            if (String.IsNullOrEmpty(contenttypes)) {
                return T("Any content item");
            }

            return T("Content with type {0}", contenttypes);
        }
    }

    public class ContentTypesFilterForms : IFormProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ContentTypesFilterForms(
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
                        Id: "AnyOfContentTypes",
                        _Parts: Shape.SelectList(
                            Id: "contenttypes", Name: "ContentTypes",
                            Title: T("Content types"),
                            Description: T("Select some content types."),
                            Size: 10,
                            Multiple: true
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(x => x.DisplayName)) {
                        f._Parts.Add(new SelectListItem { Value = contentType.Name, Text = contentType.DisplayName });
                    }

                    return f;
                };

            context.Form("ContentTypesFilter", form);

        }
    }
}