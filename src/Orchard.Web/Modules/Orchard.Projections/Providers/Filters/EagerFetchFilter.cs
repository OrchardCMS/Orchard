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
                .Element("EagerFetch", T("Eager fetch"), T("Eager fetch content part records"),
                    ApplyFilter,
                    DisplayFilter,
                    "ContentPartRecordsForm"
                );

        }

        public void ApplyFilter(FilterContext context) {
            var contentPartRecords = (string)context.State.ContentPartRecords;
            if (!String.IsNullOrEmpty(contentPartRecords)) {
                var contentParts = contentPartRecords.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                context.Query = context.Query.Include(contentParts);
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            string contentpartrecords = context.State.ContentPartRecords;

            if (String.IsNullOrEmpty(contentpartrecords)) {
                return T("No content part record");
            }

            return T("Eager fetch part records {0}", contentpartrecords);
        }
    }
}