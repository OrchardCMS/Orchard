using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;

namespace Orchard.Projections.Services {
    public interface IProjectionManager : IDependency {
        IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters();
        IEnumerable<TypeDescriptor<SortCriterionDescriptor>> DescribeSortCriteria();
        IEnumerable<TypeDescriptor<LayoutDescriptor>> DescribeLayouts();
        IEnumerable<TypeDescriptor<PropertyDescriptor>> DescribeProperties();

        FilterDescriptor GetFilter(string category, string type);
        SortCriterionDescriptor GetSortCriterion(string category, string type);
        LayoutDescriptor GetLayout(string category, string type);
        PropertyDescriptor GetProperty(string category, string type);

        IEnumerable<ContentItem> GetContentItems(int queryId, int skip = 0, int count = 0);
        int GetCount(int queryId);
    }

}