using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Models {
    public interface IContentManager : IDependency {
        ContentItem New(string contentType);
        void Create(ContentItem contentItem);

        ContentItem Get(int id);
        IEnumerable<ContentItem> Get(IEnumerable<int> id);

        IEnumerable<ModelTemplate> GetDisplays(IContent contentItem);
        IEnumerable<ModelTemplate> GetEditors(IContent contentItem);
        IEnumerable<ModelTemplate> UpdateEditors(IContent contentItem, IUpdateModel updater);
        
        IContentQuery Query();
    }
}
