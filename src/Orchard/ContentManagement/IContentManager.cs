using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Indexing;

namespace Orchard.ContentManagement {
    /// <summary>
    /// Content management functionality to deal with Orchard content items and their parts
    /// </summary>
    public interface IContentManager : IDependency {
        IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions();


        /// <summary>
        /// Instantiates a new content item with the specified type
        /// </summary>
        /// <remarks>
        /// The content item is not yet persisted!
        /// </remarks>
        /// <param name="contentType">The name of the content type</param>
        ContentItem New(string contentType);
        

        /// <summary>
        /// Creates (persists) a new content item
        /// </summary>
        /// <param name="contentItem">The content item instance filled with all necessary data</param>
        void Create(ContentItem contentItem);

        /// <summary>
        /// Creates (persists) a new content item with the specified version
        /// </summary>
        /// <param name="contentItem">The content item instance filled with all necessary data</param>
        /// <param name="options">The version to create the item with</param>
        void Create(ContentItem contentItem, VersionOptions options);


        /// <summary>
        /// Gets the content item with the specified id
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        ContentItem Get(int id);

        /// <summary>
        /// Gets the content item with the specified id and version
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        /// <param name="options">The version option</param>
        ContentItem Get(int id, VersionOptions options);

        /// <summary>
        /// Gets the content item with the specified id, version and query hints
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        /// <param name="options">The version option</param>
        /// <param name="hints">The query hints</param>
        ContentItem Get(int id, VersionOptions options, QueryHints hints);

        /// <summary>
        /// Gets all versions of the content item specified with its id
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        IEnumerable<ContentItem> GetAllVersions(int id);

        IEnumerable<T> GetMany<T>(IEnumerable<int> ids, VersionOptions options, QueryHints hints) where T : class, IContent;
        IEnumerable<T> GetManyByVersionId<T>(IEnumerable<int> versionRecordIds, QueryHints hints) where T : class, IContent;

        void Publish(ContentItem contentItem);
        void Unpublish(ContentItem contentItem);
        void Remove(ContentItem contentItem);
        void Index(ContentItem contentItem, IDocumentIndex documentIndex);

        XElement Export(ContentItem contentItem);
        void Import(XElement element, ImportContentSession importContentSession);

        /// <summary>
        /// Flushes all pending content items to the persistance layer
        /// </summary>
        void Flush();
        
        /// <summary>
        /// Clears the current referenced content items
        /// </summary>
        void Clear();

        /// <summary>
        /// Query for arbitrary content items
        /// </summary>
        IContentQuery<ContentItem> Query();
        IHqlQuery HqlQuery();

        ContentItemMetadata GetItemMetadata(IContent contentItem);
        IEnumerable<GroupInfo> GetEditorGroupInfos(IContent contentItem);
        IEnumerable<GroupInfo> GetDisplayGroupInfos(IContent contentItem);
        GroupInfo GetEditorGroupInfo(IContent contentItem, string groupInfoId);
        GroupInfo GetDisplayGroupInfo(IContent contentItem, string groupInfoId);


        /// <summary>
        /// Builds the display shape of the specified content item
        /// </summary>
        /// <param name="content">The content item to use</param>
        /// <param name="displayType">The display type (e.g. Summary, Detail) to use</param>
        /// <param name="groupId">Id of the display group (stored in the content item's metadata)</param>
        /// <returns>The display shape</returns>
        dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "");

        /// <summary>
        /// Builds the editor shape of the specified content item
        /// </summary>
        /// <param name="content">The content item to use</param>
        /// <param name="groupId">Id of the editor group (stored in the content item's metadata)</param>
        /// <returns>The editor shape</returns>
        dynamic BuildEditor(IContent content, string groupId = "");

        /// <summary>
        /// Updates the content item and its editor shape with new data through an IUpdateModel
        /// </summary>
        /// <param name="content">The content item to update</param>
        /// <param name="updater">The updater to use for updating</param>
        /// <param name="groupId">Id of the editor group (stored in the content item's metadata)</param>
        /// <returns>The updated editor shape</returns>
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
