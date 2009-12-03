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

        ItemDisplayViewModel<TContent> GetDisplayViewModel<TContent>(TContent content, string groupName, string displayType) where TContent : IContent;
        ItemEditorViewModel<TContent> GetEditorViewModel<TContent>(TContent content, string groupName) where TContent : IContent;
        ItemEditorViewModel<TContent> UpdateEditorViewModel<TContent>(TContent content, string groupName, IUpdateModel updater) where TContent : IContent;
    }
}
