using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.ViewModels;

namespace Orchard.Core.Containers.Services {
    public interface IContainerService : IDependency {
        IEnumerable<ContentTypeDefinition> GetContainableTypes();
        IEnumerable<ContentTypeDefinition> GetContainerTypes();
        IEnumerable<ContainerPart> GetContainers(VersionOptions options = null);
        IContentQuery<ContainerPart> GetContainersQuery(VersionOptions options = null);
        ContainerPart Get(int id, VersionOptions options = null);
        IEnumerable<ContentItem> GetContentItems(int containerId, VersionOptions options = null);
        int CountContentItems(int containerId, VersionOptions options = null);
        ContentItem Next(int containerId, ContainablePart current);
        ContentItem Previous(int containerId, ContainablePart current);
        ContainerPart GetContainer(IContent content, VersionOptions options = null);

        /// <summary>
        /// Returns the position of the last item.
        /// </summary>
        int GetLastPosition(int containerId);

        /// <summary>
        /// Returns the position of the first item.
        /// </summary>
        int GetFirstPosition(int containerId);

        /// <summary>
        /// Updates the path to an item. Use this method when its container changes.
        /// </summary>
        void UpdateItemPath(ContentItem item);

        void Reverse(IEnumerable<ContainablePart> items);
        void Shuffle(IEnumerable<ContainablePart> items);
        void Sort(IEnumerable<ContainablePart> items, SortBy sortBy, SortDirection sortDirection);

        void MoveItem(ContainablePart item, ContainerPart targetContainer, int? position = null);
        void UpdateItemCount(ContainerPart container);
        IContentQuery<CommonPart, ContainablePartRecord> GetContentItemsQuery(int containerId, IEnumerable<int> ids = null, VersionOptions options = null);
        IContentQuery<CommonPart, ContainablePartRecord> GetOrderedContentItemsQuery(int containerId, IEnumerable<int> ids = null, VersionOptions options = null);
    }

    public class ContainerService : IContainerService {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IRandomizer _randomizer;

        public ContainerService(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager, IRandomizer randomizer) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _randomizer = randomizer;
        }

        public IEnumerable<ContentTypeDefinition> GetContainableTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Where(td => td.Parts.Any(p => p.PartDefinition.Name == typeof(ContainablePart).Name)).OrderBy(x => x.DisplayName);
        }

        public IEnumerable<ContentTypeDefinition> GetContainerTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Where(td => td.Parts.Any(p => p.PartDefinition.Name == typeof(ContainerPart).Name)).OrderBy(x => x.DisplayName);
        }

        public IEnumerable<ContainerPart> GetContainers(VersionOptions options = null) {
            return GetContainersQuery(options).List();
        }

        public IContentQuery<ContainerPart> GetContainersQuery(VersionOptions options = null) {
            options = options ?? VersionOptions.Published;
            return _contentManager.Query<ContainerPart, ContainerPartRecord>(options);
        }

        public ContainerPart Get(int id, VersionOptions options = null) {
            options = options ?? VersionOptions.Published;
            return _contentManager.Get<ContainerPart>(id, options);
        }

        public IEnumerable<ContentItem> GetContentItems(int containerId, VersionOptions options = null) {
            options = options ?? VersionOptions.Published;
            return GetOrderedContentItemsQuery(containerId, null, options).List().Select(x => x.ContentItem);
        }

        public int CountContentItems(int containerId, VersionOptions options = null) {
            options = options ?? VersionOptions.Published;
            return GetOrderedContentItemsQuery(containerId, options: options).Count();
        }

        public ContentItem Next(int containerId, ContainablePart current) {
            if (current == null)
                return null;
            return GetOrderedContentItemsQuery(containerId, null, VersionOptions.Latest).Where(x => x.Position < current.Position).Slice(0, 1).Select(x => x.ContentItem).FirstOrDefault();
        }

        public ContentItem Previous(int containerId, ContainablePart current) {
            if (current == null)
                return null;
            return GetContentItemsQuery(containerId, null, VersionOptions.Latest).OrderBy(x => x.Position).Where(x => x.Position > current.Position).Slice(0, 1).Select(x => x.ContentItem).FirstOrDefault();
        }

        public ContainerPart GetContainer(IContent content, VersionOptions options = null) {
            options = options ?? VersionOptions.Published;
            if (content == null)
                return null;

            var commonPart = content.As<CommonPart>();
            if (commonPart == null)
                return null;

            var container = commonPart.Record.Container;
            return container == null ? null : _contentManager.Get<ContainerPart>(container.Id, options);
        }

        public int GetLastPosition(int containerId) {
            var last = GetContentItemsQuery(containerId, null, VersionOptions.Latest).OrderBy(x => x.Position).Slice(0, 1).FirstOrDefault();
            return last != null ? last.As<ContainablePart>().Position : 0;
        }

        public int GetFirstPosition(int containerId) {
            var first = GetContentItemsQuery(containerId, null, VersionOptions.Latest).OrderByDescending(x => x.Position).Slice(0, 1).FirstOrDefault();
            return first != null ? first.As<ContainablePart>().Position : 0;
        }

        public IContentQuery<CommonPart, ContainablePartRecord> GetContentItemsQuery(int containerId, IEnumerable<int> ids = null, VersionOptions options = null) {
            options = options ?? VersionOptions.Published;
            var query = _contentManager.Query<CommonPart, CommonPartRecord>(options);
            query = ids == null ? query.Where(x => x.Container.Id == containerId) : query.Where(x => (x.Container.Id == containerId || x.Container == null) && ids.Contains(x.Id));

            return query.Join<ContainablePartRecord>();
        }

        public IContentQuery<CommonPart, ContainablePartRecord> GetOrderedContentItemsQuery(int containerId, IEnumerable<int> ids = null, VersionOptions options = null) {
            return GetContentItemsQuery(containerId, ids, options).OrderByDescending(x => x.Position);
        }

        public void Reverse(IEnumerable<ContainablePart> items) {
            var list = UpdatePositions(items).ToArray();
            Array.Reverse(list);
            UpdatePositions(list);
        }

        public void Shuffle(IEnumerable<ContainablePart> items) {
            var list = UpdatePositions(items).ToArray();
            Shuffle(list);
            UpdatePositions(list);
        }

        public void Sort(IEnumerable<ContainablePart> items, SortBy sortBy, SortDirection sortDirection) {
            var list = items.ToArray();
            switch (sortBy) {
                case SortBy.Created:
                    Sort(list, sortDirection, (a, b) => a.As<CommonPart>().CreatedUtc.GetValueOrDefault().CompareTo(b.As<CommonPart>().CreatedUtc.GetValueOrDefault()));
                    break;
                case SortBy.Modified:
                    Sort(list, sortDirection, (a, b) => a.As<CommonPart>().ModifiedUtc.GetValueOrDefault().CompareTo(b.As<CommonPart>().ModifiedUtc.GetValueOrDefault()));
                    break;
                case SortBy.Published:
                    Sort(list, sortDirection, (a, b) => a.As<CommonPart>().PublishedUtc.GetValueOrDefault().CompareTo(b.As<CommonPart>().PublishedUtc.GetValueOrDefault()));
                    break;
                case SortBy.DisplayText:
                    Sort(list, sortDirection, (a, b) => String.CompareOrdinal(_contentManager.GetItemMetadata(a).DisplayText, _contentManager.GetItemMetadata(b).DisplayText));
                    break;
            }
            UpdatePositions(list);
        }

        public void MoveItem(ContainablePart item, ContainerPart targetContainer, int? position = null) {
            var commonPart = item.As<CommonPart>();

            if(commonPart == null)
                throw new ArgumentException("Cannot move content that have no CommonPart", "item");

            var previousContainer = commonPart.Container != null ? commonPart.Container.As<ContainerPart>() : default(ContainerPart);
            commonPart.Container = targetContainer;

            if (previousContainer != null && previousContainer.Id != targetContainer.Id)
                UpdateItemCount(previousContainer);

            if (position != null)
                item.Position = position.Value;

            UpdateItemCount(targetContainer);
            UpdateItemPath(item.ContentItem);
        }

        public void UpdateItemCount(ContainerPart container) {
            if(container == null) throw new ArgumentNullException("container");
            container.ItemCount = CountContentItems(container.Id, VersionOptions.Published);
        }

        /// <summary>
        /// Updates the path to an item. Use this method when its container changes.
        /// </summary>
        public void UpdateItemPath(ContentItem item) {
            // Fixes an Item's path when its container changes.
            // Force a publish/unpublish event so AutoroutePart fixes the content items path and the paths of any child objects if it is also a container.
            if (item.VersionRecord.Published) {
                item.VersionRecord.Published = false;
                _contentManager.Publish(item);
            }
            else {
                item.VersionRecord.Published = true;
                _contentManager.Unpublish(item);
            }
        }

        private void Sort<TValue>(TValue[] array, SortDirection direction, Func<TValue, TValue, int> sort) {
            Array.Sort(array, Sort(direction, sort));
        }

        private Comparison<TValue> Sort<TValue>(SortDirection direction, Func<TValue, TValue, int> sort) {
            return (a, b) => direction == SortDirection.Ascending ? sort(a, b) : sort(b, a);
        }

        private IList<ContainablePart> UpdatePositions(IEnumerable<ContainablePart> items) {
            var list = items.ToList();
            var index = list.Count;
            foreach (var item in list) {
                item.Position = --index;
            }
            return list;
        }

        private void Shuffle<T>(IList<T> source) {
            var n = source.Count;
            for (var i = 0; i < n; i++) {
                var r = i + _randomizer.Next(0, n - i);
                var temp = source[i];
                source[i] = source[r];
                source[r] = temp;
            }
        }
    }
}