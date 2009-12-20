using System.Collections.Generic;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Models {
    public interface IContentManager : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        ContentItem New(string contentType);
        void Create(ContentItem contentItem);

        ContentItem Get(int id);

        IContentQuery<ContentItem> Query();

        ContentItemMetadata GetItemMetadata(IContent contentItem);

        ItemDisplayModel<TContent> BuildDisplayModel<TContent>(TContent content, string groupName, string displayType) where TContent : IContent;
        ItemDisplayModel<TContent> BuildDisplayModel<TContent>(TContent content, string groupName, string displayType, string templatePath) where TContent : IContent;
        ItemEditorModel<TContent> BuildEditorModel<TContent>(TContent content, string groupName) where TContent : IContent;
        ItemEditorModel<TContent> BuildEditorModel<TContent>(TContent content, string groupName, string templatePath) where TContent : IContent;
        ItemEditorModel<TContent> UpdateEditorModel<TContent>(TContent content, string groupName, IUpdateModel updater) where TContent : IContent;
        ItemEditorModel<TContent> UpdateEditorModel<TContent>(TContent content, string groupName, IUpdateModel updater, string templatePath) where TContent : IContent;
    }
}
