using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Models {
    public interface IContentManager : IDependency {
        ContentItem New(string contentType);
        ContentItem Get(int id);
        void Create(ContentItem contentItem);

        IEnumerable<ModelTemplate> GetEditors(ContentItem contentItem);
        IEnumerable<ModelTemplate> UpdateEditors(ContentItem contentItem, IUpdateModel updater);
    }
}
