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
        ItemDisplayViewModel<TContentPart> GetDisplays<TContentPart>(TContentPart contentItem, string groupName, string displayType) where TContentPart : IContent;
        ItemEditorViewModel<TContentPart> GetEditors<TContentPart>(TContentPart contentItem, string groupName) where TContentPart : IContent;
        ItemEditorViewModel<TContentPart> UpdateEditors<TContentPart>(TContentPart contentItem, string groupName, IUpdateModel updater) where TContentPart : IContent;
    }
}
