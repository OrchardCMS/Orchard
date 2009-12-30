using System.Collections.Generic;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement {
    public interface IContentManager : IDependency {
        IEnumerable<ContentType> GetContentTypes();

        ContentItem New(string contentType);
        
        void Create(ContentItem contentItem);
        void Create(ContentItem contentItem, VersionOptions options);

        ContentItem Get(int id);
        ContentItem Get(int id, VersionOptions options);

        ContentItem AppendLatestVersion(ContentItem sourceVersion);

        IContentQuery<ContentItem> Query();

        ContentItemMetadata GetItemMetadata(IContent contentItem);

        ItemDisplayModel<TContent> BuildDisplayModel<TContent>(TContent content, string displayType) where TContent : IContent;
        ItemEditorModel<TContent> BuildEditorModel<TContent>(TContent content) where TContent : IContent;
        ItemEditorModel<TContent> UpdateEditorModel<TContent>(TContent content, IUpdateModel updater) where TContent : IContent;
    }

    public class VersionOptions {
        public static VersionOptions Latest { get { return new VersionOptions { IsLatest = true }; } }
        public static VersionOptions Published { get { return new VersionOptions { IsPublished = true }; } }
        public static VersionOptions Draft { get { return new VersionOptions { IsDraft = true }; } }
        public static VersionOptions DraftRequired { get { return new VersionOptions { IsDraft = true, IsDraftRequired = true }; } }
        public static VersionOptions Number(int version) { return new VersionOptions { VersionNumber = version }; }
        public static VersionOptions VersionRecord(int id) { return new VersionOptions { VersionRecordId = id }; }

        public bool IsLatest { get; private set; }
        public bool IsPublished { get; private set; }
        public bool IsDraft { get; private set; }
        public bool IsDraftRequired { get; private set; }
        public int VersionNumber { get; private set; }
        public int VersionRecordId { get; private set; }
    }
}
