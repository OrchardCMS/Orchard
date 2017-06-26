using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;

namespace Orchard.Projections.Services {
    public interface IProjectionManagerExtension : IProjectionManager {

        IEnumerable<ContentItem> GetContentItems(int queryId, ContentPart part, int skip = 0, int count = 0);
        int GetCount(int queryId, ContentPart part);
    }

}