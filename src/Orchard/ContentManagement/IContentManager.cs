using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Indexing;

namespace Orchard.ContentManagement {
    public interface IContentManager : IDependency {
        IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions();

        ContentItem New(string contentType);
        
        void Create(ContentItem contentItem);
        void Create(ContentItem contentItem, VersionOptions options);

        ContentItem Get(int id);
        ContentItem Get(int id, VersionOptions options);
        IEnumerable<ContentItem> GetAllVersions(int id);

        IEnumerable<T> GetMany<T>(IEnumerable<int> ids, VersionOptions options, QueryHints hints) where T : class, IContent;
        IEnumerable<T> GetManyByVersionId<T>(IEnumerable<int> versionRecordIds, QueryHints hints) where T : class, IContent;

        void Publish(ContentItem contentItem);
        void Unpublish(ContentItem contentItem);
        void Remove(ContentItem contentItem);
        void Index(ContentItem contentItem, IDocumentIndex documentIndex);

        XElement Export(ContentItem contentItem);
        void Import(XElement element, ImportContentSession importContentSession);

        void Flush();
        IContentQuery<ContentItem> Query();

        ContentItemMetadata GetItemMetadata(IContent contentItem);
        IEnumerable<GroupInfo> GetEditorGroupInfos(IContent contentItem);
        IEnumerable<GroupInfo> GetDisplayGroupInfos(IContent contentItem);
        GroupInfo GetEditorGroupInfo(IContent contentItem, string groupInfoId);
        GroupInfo GetDisplayGroupInfo(IContent contentItem, string groupInfoId);

        dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "");
        dynamic BuildEditor(IContent content, string groupId = "");
        dynamic UpdateEditor(IContent content, IUpdateModel updater, string groupId = "");
    }

    public interface IContentDisplay : IDependency {
        dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "");
        dynamic BuildEditor(IContent content, string groupId = "");
        dynamic UpdateEditor(IContent content, IUpdateModel updater, string groupId = "");
    }

    public class VersionOptions {
        /// <summary>
        /// Gets the latest version.
        /// </summary>
        public static VersionOptions Latest { get { return new VersionOptions { IsLatest = true }; } }

        /// <summary>
        /// Gets the latest published version.
        /// </summary>
        public static VersionOptions Published { get { return new VersionOptions { IsPublished = true }; } }

        /// <summary>
        /// Gets the latest draft version.
        /// </summary>
        public static VersionOptions Draft { get { return new VersionOptions { IsDraft = true }; } }

        /// <summary>
        /// Gets the latest version and creates a new version draft based on it.
        /// </summary>
        public static VersionOptions DraftRequired { get { return new VersionOptions { IsDraft = true, IsDraftRequired = true }; } }

        /// <summary>
        /// Gets all versions.
        /// </summary>
        public static VersionOptions AllVersions { get { return new VersionOptions { IsAllVersions = true }; } }

        /// <summary>
        /// Gets a specific version based on its number.
        /// </summary>
        public static VersionOptions Number(int version) { return new VersionOptions { VersionNumber = version }; }

        /// <summary>
        /// Gets a specific version based on the version record identifier.
        /// </summary>
        public static VersionOptions VersionRecord(int id) { return new VersionOptions { VersionRecordId = id }; }

        public bool IsLatest { get; private set; }
        public bool IsPublished { get; private set; }
        public bool IsDraft { get; private set; }
        public bool IsDraftRequired { get; private set; }
        public bool IsAllVersions { get; private set; }
        public int VersionNumber { get; private set; }
        public int VersionRecordId { get; private set; }
    }
}
