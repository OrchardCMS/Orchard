using System;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using IFilterProvider = Orchard.Projections.Services.IFilterProvider;

namespace Orchard.Projections.Providers.Filters {
    public class EagerFectchFilter : IFilterProvider {
        public EagerFectchFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeFilterContext describe) {
            describe.For("Content", T("Content"),T("Content"))
                .Element("EagerFetch", T("Eager fetch"), T("Eager fetch content parts"),
                    ApplyFilter,
                    DisplayFilter,
                    "ContentPartsForm"
                );

        }

        public void ApplyFilter(FilterContext context) {
            var contentPartNames = (string)context.State.ContentParts;
            if (!String.IsNullOrEmpty(contentPartNames)) {
                var contentParts = contentPartNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                context.Query = context.Query.Include(contentParts);
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            string contentparts = context.State.ContentParts;

            if (String.IsNullOrEmpty(contentparts)) {
                return T("No content part");
            }

            return T("Eager fetch parts {0}", contentparts);
        }
    }
}