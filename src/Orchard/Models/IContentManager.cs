using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement {
    public interface IContentManager : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        ContentItem New(string contentType);
        void Create(ContentItem contentItem);

        ContentItem Get(int id);

        IContentQuery<ContentItem> Query();

        ContentItemMetadata GetItemMetadata(IContent contentItem);

        ItemDisplayModel<TContent> BuildDisplayModel<TContent>(TContent content, string displayType) where TContent : IContent;
        ItemEditorModel<TContent> BuildEditorModel<TContent>(TContent content) where TContent : IContent;
        ItemEditorModel<TContent> UpdateEditorModel<TContent>(TContent content, IUpdateModel updater) where TContent : IContent;
    }
}
