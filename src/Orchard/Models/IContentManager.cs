using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Models.ViewModels;

namespace Orchard.Models {
    public interface IContentManager : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        ContentItem New(string contentType);
        void Create(ContentItem contentItem);

        ContentItem Get(int id);

        IContentQuery<ContentItem> Query();

        ContentItemMetadata GetItemMetadata(IContent contentItem);
        ItemDisplayViewModel GetDisplays(IContent contentItem, string tabName, string displayType);
        ItemEditorViewModel GetEditors(IContent contentItem, string tabName);
        ItemEditorViewModel UpdateEditors(IContent contentItem, string tabName, IUpdateModel updater);
    }
}
