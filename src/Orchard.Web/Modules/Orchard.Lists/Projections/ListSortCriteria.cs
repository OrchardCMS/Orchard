using System;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Models;
using Orchard.Lists.Services;
using Orchard.Localization;

namespace Orchard.Lists.Projections {
    public class ListSortCriteria : Component, ISortCriterionProvider {

        public void Describe(dynamic describe) {
            describe.For("List", new LocalizedString("List"), T("List item position"))
                .Element("Position", T("Position"), T("Sorts the results by position"),
                (Action<dynamic>)ApplySortCriterion,
                (Func<dynamic, LocalizedString>)DisplaySortCriterion,
                "SortOrder"
            );
        }

        public void ApplySortCriterion(dynamic context) {
            bool ascending = Boolean.Parse(Convert.ToString(context.State.Sort));
            var query = (IHqlQuery)context.Query;
            query = ascending
                ? query.OrderBy(alias => alias.ContentPartRecord<ContainablePartRecord>(), x => x.Desc("Position"))
                : query.OrderBy(alias => alias.ContentPartRecord<ContainablePartRecord>(), x => x.Asc("Position"));

            context.Query = query;
        }

        public LocalizedString DisplaySortCriterion(dynamic context) {
            bool ascending = Boolean.Parse(Convert.ToString(context.State.Sort));
            return T(@ascending ? "Ordered by {0}, ascending" : "Ordered by {0}, descending", T("Position"));
        }
    }
}