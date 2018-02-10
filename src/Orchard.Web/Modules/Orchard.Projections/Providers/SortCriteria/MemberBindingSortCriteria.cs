using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Projections.Providers.SortCriteria {
    public class MemberBindingSortCriteria : ISortCriterionProvider {
        private readonly IEnumerable<IMemberBindingProvider> _bindingProviders;

        public MemberBindingSortCriteria(IEnumerable<IMemberBindingProvider> bindingProviders) {
            _bindingProviders = bindingProviders;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeSortCriterionContext describe) {
            var builder = new BindingBuilder();

            foreach(var bindingProvider in _bindingProviders) {
                bindingProvider.GetMemberBindings(builder);
            }

            var groupedMembers = builder.Build().GroupBy(b => b.Property.DeclaringType).ToDictionary(b => b.Key, b => b);

            foreach (var typeMembers in groupedMembers.Keys) {
                var descriptor = describe.For(typeMembers.Name, new LocalizedString(typeMembers.Name.CamelFriendly()), T("Members for {0}", typeMembers.Name));
                foreach (var member in groupedMembers[typeMembers]) {
                    var closureMember = member;
                    descriptor.Element(member.Property.Name, member.DisplayName, member.Description,
                        context => ApplyFilter(context, closureMember.Property),
                        context => DisplaySortCriterion(context, closureMember.DisplayName.Text),
                        SortCriterionFormProvider.FormName
                    );
                }
            }
        }

        public void ApplyFilter(SortCriterionContext context, PropertyInfo property) {

            bool ascending = Boolean.Parse(Convert.ToString(context.State.Sort));
            context.Query = ascending
                ? context.Query.OrderBy(alias => alias.ContentPartRecord(property.DeclaringType), x => x.Asc(property.Name))
                : context.Query.OrderBy(alias => alias.ContentPartRecord(property.DeclaringType), x => x.Desc(property.Name));
        }

        public LocalizedString DisplaySortCriterion(SortCriterionContext context, string propertyName) {
            bool ascending = Boolean.Parse(Convert.ToString(context.State.Sort));

            if (ascending) {
                return T("Ordered by {0}, ascending", propertyName);
            }

            return T("Ordered by {0}, descending", propertyName);
        }

    }
}