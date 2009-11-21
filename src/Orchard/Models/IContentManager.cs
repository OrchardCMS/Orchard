using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Models {
    public interface IContentManager : IDependency {
        IContent New(string contentType);
        IContent Get(int id);
        void Create(IContent contentItem);

        IEnumerable<ModelTemplate> GetDisplays(IContent contentItem);
        IEnumerable<ModelTemplate> GetEditors(IContent contentItem);
        IEnumerable<ModelTemplate> UpdateEditors(IContent contentItem, IUpdateModel updater);
    }
}
