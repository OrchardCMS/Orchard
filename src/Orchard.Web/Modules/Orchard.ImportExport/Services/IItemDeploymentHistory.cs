using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IItemDeploymentHistory : IDependency {
        IEnumerable<ItemDeploymentEntry> GetHistory(IContent contentItem);
        void AddEntry(IContent contentItem, ItemDeploymentEntry entry);
    }
}
