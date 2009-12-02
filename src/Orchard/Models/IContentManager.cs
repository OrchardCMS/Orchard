using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.UI.Models;

namespace Orchard.Models {
    public interface IContentManager : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        ContentItem New(string contentType);
        void Create(ContentItem contentItem);

        ContentItem Get(int id);

        IContentQuery<ContentItem> Query();

        ContentItemMetadata GetItemMetadata(IContent contentItem);
        IEnumerable<ModelTemplate> GetDisplays(IContent contentItem);
        IEnumerable<ModelTemplate> GetEditors(IContent contentItem);
        IEnumerable<ModelTemplate> UpdateEditors(IContent contentItem, IUpdateModel updater);
    }
}
